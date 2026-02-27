using System.Text;
using Application.DTOs.AI;
using Application.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class AiModuleService
    {
        private const int MaxModuleContextChars = 12000;
        private const int MaxHistoryMessages = 12;
        private const int MaxMessageLength = 4000;
        private const int MinTextLength = 10;
        private const int MinQuizTextLength = 30;

        private readonly ICourseModuleRepository _courseModuleRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly AiService _aiService;
        private readonly AiConversationMemoryService _conversationMemory;
        private readonly ILogger<AiModuleService> _logger;

        public AiModuleService(
            ICourseModuleRepository courseModuleRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            AiService aiService,
            AiConversationMemoryService conversationMemory,
            ILogger<AiModuleService> logger)
        {
            _courseModuleRepository = courseModuleRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _aiService = aiService;
            _conversationMemory = conversationMemory;
            _logger = logger;
        }

        public async Task<AiTextResponseDto> GenerateModuleSummaryAsync(
            int moduleId,
            int userId,
            string role,
            AiModuleSummaryRequestDto request,
            CancellationToken cancellationToken = default)
        {
            ValidateSummaryRequest(request);

            var module = await GetAuthorizedModuleAsync(moduleId, userId, role);
            var rawContext = BuildModuleContext(module);

            if (string.IsNullOrWhiteSpace(rawContext) || rawContext.Length < MinTextLength)
            {
                throw new BadRequestException("This module has insufficient text content to summarize.");
            }

            var content = BuildSummaryContext(module, request.Mode);

            var summaryRequest = new AiSummaryRequestDto
            {
                Text = content,
                MaxBullets = request.MaxBullets,
                Language = request.Language ?? "en",
                Mode = request.Mode
            };

            try
            {
                return await _aiService.SummarizeAsync(summaryRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Summary generation failed for module {ModuleId}", moduleId);
                return CreateSafeFallbackResponse("summary");
            }
        }

        public async Task<AiTextResponseDto> GenerateModuleQuizAsync(
            int moduleId,
            int userId,
            string role,
            AiModuleQuizRequestDto request,
            CancellationToken cancellationToken = default)
        {
            ValidateQuizRequest(request);

            var module = await GetAuthorizedModuleAsync(moduleId, userId, role);
            var rawContext = BuildModuleContext(module);

            if (string.IsNullOrWhiteSpace(rawContext) || rawContext.Length < MinQuizTextLength)
            {
                throw new BadRequestException("This module has insufficient text content to generate a quiz.");
            }

            var content = BuildQuizContext(module);

            var quizRequest = new AiQuizRequestDto
            {
                Text = content,
                QuestionsCount = request.QuestionsCount,
                Language = request.Language ?? "en",
                Difficulty = request.Difficulty,
                IncludeExplanations = request.IncludeExplanations
            };

            try
            {
                return await _aiService.GenerateQuizAsync(quizRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Quiz generation failed for module {ModuleId}", moduleId);
                return CreateSafeFallbackResponse("quiz");
            }
        }

        public async Task<AiTextResponseDto> ChatOnModuleAsync(
            int moduleId,
            int userId,
            string role,
            AiModuleChatRequestDto request,
            CancellationToken cancellationToken = default)
        {
            ValidateChatRequest(request);

            var module = await GetAuthorizedModuleAsync(moduleId, userId, role);
            var moduleContext = BuildModuleContext(module);

            if (string.IsNullOrWhiteSpace(moduleContext) || moduleContext.Length < MinTextLength)
            {
                throw new BadRequestException("This module has insufficient text content for grounded chat.");
            }

            moduleContext = TrimToMaxChars(moduleContext, MaxModuleContextChars);

            var conversationId = string.IsNullOrWhiteSpace(request.ConversationId)
                ? Guid.NewGuid().ToString("N")
                : request.ConversationId.Trim();

            var incomingHistory = NormalizeHistory(request.History);
            var conversationKey = BuildConversationKey(moduleId, userId, conversationId);

            List<AiChatMessageDto> history;
            if (request.UseServerMemory)
            {
                var serverHistory = _conversationMemory.GetHistory(conversationKey);
                history = serverHistory.Concat(incomingHistory).ToList();
            }
            else
            {
                history = incomingHistory;
            }

            history = NormalizeHistory(history);
            if (history.Count > MaxHistoryMessages)
            {
                history = history.Skip(history.Count - MaxHistoryMessages).ToList();
            }

            var systemPrompt = BuildGroundedChatSystemPrompt(module, request.Language);
            var userMessage = BuildGroundedChatUserMessage(moduleContext, request.Message);

            var chatRequest = new AiChatRequestDto
            {
                Message = userMessage,
                Language = request.Language ?? "en",
                History = history,
                Context = systemPrompt,
                StrictGrounded = true
            };

            try
            {
                var response = await _aiService.ChatAsync(chatRequest, cancellationToken);
                if (response == null || string.IsNullOrWhiteSpace(response.Output))
                {
                    _logger.LogWarning("Empty response from AI chat for module {ModuleId}", moduleId);
                    var emptyFallback = CreateGroundedFallbackResponse(module);
                    emptyFallback.ConversationId = conversationId;
                    return emptyFallback;
                }

                if (request.UseServerMemory)
                {
                    var updatedHistory = history
                        .Append(new AiChatMessageDto { Role = "user", Content = request.Message.Trim() })
                        .Append(new AiChatMessageDto { Role = "assistant", Content = response.Output })
                        .ToList();

                    _conversationMemory.SaveHistory(conversationKey, updatedHistory);
                }

                response.ConversationId = conversationId;

                _logger.LogInformation(
                    "Module chat success for user {UserId}, module {ModuleId}, conversation {ConversationId}, historyCount {HistoryCount}",
                    userId,
                    moduleId,
                    conversationId,
                    history.Count);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Module chat failed for user {UserId}, module {ModuleId}, conversation {ConversationId}",
                    userId,
                    moduleId,
                    conversationId);

                var fallback = CreateGroundedFallbackResponse(module);
                fallback.ConversationId = conversationId;
                return fallback;
            }
        }

        #region Validation Methods

        private static void ValidateSummaryRequest(AiModuleSummaryRequestDto request)
        {
            if (request == null)
            {
                throw new BadRequestException("Request cannot be null.");
            }

            if (request.MaxBullets < 3 || request.MaxBullets > 15)
            {
                throw new BadRequestException("Max bullets must be between 3 and 15.");
            }

            if (!string.IsNullOrEmpty(request.Language) && request.Language.Length > 8)
            {
                throw new BadRequestException("Language code cannot exceed 8 characters.");
            }
        }

        private static void ValidateQuizRequest(AiModuleQuizRequestDto request)
        {
            if (request == null)
            {
                throw new BadRequestException("Request cannot be null.");
            }

            if (request.QuestionsCount < 3 || request.QuestionsCount > 15)
            {
                throw new BadRequestException("Questions count must be between 3 and 15.");
            }

            if (!string.IsNullOrEmpty(request.Language) && request.Language.Length > 8)
            {
                throw new BadRequestException("Language code cannot exceed 8 characters.");
            }
        }

        private static void ValidateChatRequest(AiModuleChatRequestDto request)
        {
            if (request == null)
            {
                throw new BadRequestException("Request cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new BadRequestException("Message cannot be empty.");
            }

            var trimmedMessage = request.Message.Trim();
            if (trimmedMessage.Length > MaxMessageLength)
            {
                throw new BadRequestException($"Message cannot exceed {MaxMessageLength} characters.");
            }

            if (trimmedMessage.Length < 2)
            {
                throw new BadRequestException("Message must be at least 2 characters.");
            }

            if (ContainsSuspiciousPattern(trimmedMessage))
            {
                throw new BadRequestException("Message contains suspicious patterns.");
            }

            if (!string.IsNullOrEmpty(request.Language) && request.Language.Length > 8)
            {
                throw new BadRequestException("Language code cannot exceed 8 characters.");
            }

            if (request.History != null)
            {
                foreach (var msg in request.History)
                {
                    if (string.IsNullOrWhiteSpace(msg.Content))
                    {
                        throw new BadRequestException("History messages cannot have empty content.");
                    }

                    if (msg.Content.Length > MaxMessageLength)
                    {
                        throw new BadRequestException("History message content exceeds maximum length.");
                    }
                }
            }
        }

        private static bool ContainsSuspiciousPattern(string message)
        {
            var suspiciousPatterns = new[]
            {
                "ignore previous instructions",
                "disregard all previous",
                "you are now a different",
                "new system prompt",
                "<script>alert(",
                "javascript:alert("
            };

            var lowerMessage = message.ToLowerInvariant();
            return suspiciousPatterns.Any(pattern => lowerMessage.Contains(pattern));
        }

        #endregion

        #region Context Building Methods

        private static string BuildModuleContext(CourseModule module)
        {
            var buffer = new StringBuilder();
            buffer.AppendLine($"Module Title: {module.Name}");
            buffer.AppendLine($"Module Description: {module.Description}");

            if (module.ModuleContents == null)
            {
                return buffer.ToString();
            }

            foreach (var content in module.ModuleContents)
            {
                if (string.IsNullOrWhiteSpace(content?.Content))
                {
                    continue;
                }

                buffer.AppendLine($"Section: {content.Name}");
                buffer.AppendLine(content.Content);
            }

            return buffer.ToString();
        }

        private static string BuildSummaryContext(CourseModule module, SummaryMode mode)
        {
            var buffer = new StringBuilder();
            var modeDescription = mode == SummaryMode.Detailed
                ? "Provide a detailed, comprehensive summary in paragraph form."
                : "Provide a concise summary with key bullet points.";

            buffer.AppendLine("Context: Course module from MiniCoursera learning platform.");
            buffer.AppendLine($"Instruction: {modeDescription}");
            buffer.AppendLine("Focus on key concepts, definitions, and learning objectives.\n");
            buffer.AppendLine($"Module: {module.Name}");
            buffer.AppendLine($"Description: {module.Description}\n");

            if (module.ModuleContents != null)
            {
                buffer.AppendLine("Content sections:");
                foreach (var content in module.ModuleContents.Where(c => !string.IsNullOrWhiteSpace(c?.Content)))
                {
                    buffer.AppendLine($"\n=== {content.Name} ===");
                    buffer.AppendLine(content.Content);
                }
            }

            return buffer.ToString();
        }

        private static string BuildQuizContext(CourseModule module)
        {
            var buffer = new StringBuilder();
            buffer.AppendLine("Context: Generate a multiple-choice quiz based on the following course module content.");
            buffer.AppendLine("Instructions:");
            buffer.AppendLine("- Create questions that test understanding, application, and analysis");
            buffer.AppendLine("- Include 4 options per question with one correct answer");
            buffer.AppendLine("- Make questions relevant to the actual content\n");
            buffer.AppendLine($"Module: {module.Name}");
            buffer.AppendLine($"Description: {module.Description}\n");

            if (module.ModuleContents != null)
            {
                buffer.AppendLine("Content to base quiz on:");
                foreach (var content in module.ModuleContents.Where(c => !string.IsNullOrWhiteSpace(c?.Content)))
                {
                    buffer.AppendLine($"\n=== {content.Name} ===");
                    buffer.AppendLine(content.Content);
                }
            }

            return buffer.ToString();
        }

        private static string BuildGroundedChatSystemPrompt(CourseModule module, string? language)
        {
            var languageName = GetLanguageName(language);

            return $@"You are a module-grounded tutor for MiniCoursera, an online learning platform.
Your role is to help students understand the course material by answering questions based ONLY on the provided module context.

## Guidelines:
1. Answer questions using ONLY information from the module context provided
2. If the answer cannot be derived from the context, explicitly state: 'This information is not covered in the module. Please review the module content for more details.'
3. Be encouraging and supportive
4. Use clear, simple language
5. Provide examples when helpful
6. If asked about topics not covered in the module, suggest which part of the module to review

## Current Module:
Title: {module.Name}
Description: {module.Description}

## Language:
Respond in {languageName}.";
        }

        private static string BuildGroundedChatUserMessage(string moduleContext, string userQuestion)
        {
            return $@"Use the module context below to answer the student's question.

## Module Context:
{moduleContext}

## Student Question:
{userQuestion}

Answer based on the module context:";
        }

        private static string GetLanguageName(string? languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return "English";
            }

            return languageCode.ToLowerInvariant() switch
            {
                "en" => "English",
                "fr" => "French",
                "es" => "Spanish",
                "de" => "German",
                "ar" => "Arabic",
                "zh" => "Chinese",
                "ja" => "Japanese",
                "pt" => "Portuguese",
                "it" => "Italian",
                "ru" => "Russian",
                _ => "English"
            };
        }

        #endregion

        #region Fallback Methods

        private static AiTextResponseDto CreateSafeFallbackResponse(string type)
        {
            var fallbackMessages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["summary"] = "I apologize, but I'm temporarily unable to generate a summary. Please try again in a moment. In the meantime, review the module title and description to get an overview of the content.",
                ["quiz"] = "I apologize, but I'm temporarily unable to generate a quiz. Please try again in a moment. To practice, try creating your own questions based on the module content.",
                ["chat"] = "AI chat is temporarily unavailable. Please try again in a moment. For now, review the module title, description, and content sections to continue learning."
            };

            return new AiTextResponseDto
            {
                Provider = "backend-fallback",
                Model = "safe-fallback",
                Output = fallbackMessages.GetValueOrDefault(type, "An error occurred. Please try again.")
            };
        }

        private static AiTextResponseDto CreateGroundedFallbackResponse(CourseModule module)
        {
            var moduleName = module.Name ?? "this module";
            var moduleDescription = module.Description ?? "the course content";

            return new AiTextResponseDto
            {
                Provider = "backend-fallback",
                Model = "grounded-fallback",
                Output = $"I apologize, but I'm temporarily unable to process your question. For now, please review '{moduleName}' - {moduleDescription}. Try asking a question about a specific concept from the module content."
            };
        }

        #endregion

        #region Helper Methods

        private static List<AiChatMessageDto> NormalizeHistory(IEnumerable<AiChatMessageDto>? history)
        {
            if (history == null)
            {
                return new List<AiChatMessageDto>();
            }

            var normalized = new List<AiChatMessageDto>();
            foreach (var message in history)
            {
                if (message == null || string.IsNullOrWhiteSpace(message.Content))
                {
                    continue;
                }

                var role = NormalizeRole(message.Role);
                var content = message.Content.Trim();
                if (content.Length > MaxMessageLength)
                {
                    content = content[..MaxMessageLength];
                }

                normalized.Add(new AiChatMessageDto
                {
                    Role = role,
                    Content = content
                });
            }

            return normalized;
        }

        private static string NormalizeRole(string? role)
        {
            var normalizedRole = role?.Trim().ToLowerInvariant();
            return normalizedRole == "assistant" ? "assistant" : "user";
        }

        private static string BuildConversationKey(int moduleId, int userId, string conversationId)
            => $"module:{moduleId}:user:{userId}:conversation:{conversationId}";

        private async Task<CourseModule> GetAuthorizedModuleAsync(int moduleId, int userId, string role)
        {
            var module = await _courseModuleRepository.GetByIdWithContentsAsync(moduleId);
            if (module == null)
            {
                throw new NotFoundException($"Course module with ID {moduleId} was not found.");
            }

            if (role == "Student")
            {
                var enrolled = await _enrollmentRepository.IsStudentEnrolledInCourse(userId, module.CourseId);
                if (!enrolled)
                {
                    throw new ForbiddenException($"You must be enrolled in course {module.CourseId} to use AI on this module.");
                }
            }
            else if (role == "Instructor")
            {
                var ownsCourse = await _courseRepository.IsCourseCreatedByInstructor(userId, module.CourseId);
                if (!ownsCourse)
                {
                    throw new ForbiddenException($"You are not the instructor of course {module.CourseId}.");
                }
            }
            else
            {
                throw new ForbiddenException("Only students and instructors can use this AI feature.");
            }

            return module;
        }

        private static string TrimToMaxChars(string value, int maxChars)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxChars)
            {
                return value;
            }

            return value[..maxChars];
        }

        #endregion
    }
}
