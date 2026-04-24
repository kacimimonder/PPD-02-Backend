using System.Diagnostics;
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
        private readonly QuizProgressService _quizProgressService;
        private readonly AiService _aiService;
        private readonly AiConversationMemoryService _conversationMemory;
        private readonly ILogger<AiModuleService> _logger;

        public AiModuleService(
            ICourseModuleRepository courseModuleRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            QuizProgressService quizProgressService,
            AiService aiService,
            AiConversationMemoryService conversationMemory,
            ILogger<AiModuleService> logger)
        {
            _courseModuleRepository = courseModuleRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _quizProgressService = quizProgressService;
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
            var stopwatch = Stopwatch.StartNew();
            ValidateSummaryRequest(request);

            var module = await GetAuthorizedModuleAsync(moduleId, userId, role);
            var rawContext = BuildModuleContext(module);

            if (string.IsNullOrWhiteSpace(rawContext) || rawContext.Length < MinTextLength)
            {
                throw new BadRequestException(
                    "This module doesn't have enough content to generate a summary yet. " +
                    "Please ensure the module has descriptive content added before requesting a summary.");
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
                var response = await _aiService.SummarizeAsync(summaryRequest, cancellationToken);
                stopwatch.Stop();
                response.DurationMs = stopwatch.ElapsedMilliseconds;
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Summary generation failed for module {ModuleId} after {DurationMs}ms",
                    moduleId, stopwatch.ElapsedMilliseconds);
                var fallback = CreateSafeFallbackResponse("summary", module);
                fallback.DurationMs = stopwatch.ElapsedMilliseconds;
                return fallback;
            }
        }

        public async Task<AiTextResponseDto> GenerateModuleQuizAsync(
            int moduleId,
            int userId,
            string role,
            AiModuleQuizRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            ValidateQuizRequest(request);

            var module = await GetAuthorizedModuleAsync(moduleId, userId, role);
            var rawContext = BuildModuleContext(module);

            if (string.IsNullOrWhiteSpace(rawContext) || rawContext.Length < MinQuizTextLength)
            {
                throw new BadRequestException(
                    "This module needs more content before a quiz can be generated. " +
                    "Please add detailed lesson material (at least a few paragraphs) to create meaningful quiz questions.");
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
                var response = await _aiService.GenerateQuizAsync(quizRequest, cancellationToken);

                try
                {
                    var persistedQuiz = await _quizProgressService.PersistGeneratedQuizAsync(
                        moduleId,
                        userId,
                        role,
                        request,
                        response.Output);
                    response.QuizId = persistedQuiz.Id;
                }
                catch (Exception persistenceException)
                {
                    _logger.LogWarning(
                        persistenceException,
                        "Quiz generation succeeded but persistence failed for module {ModuleId}, user {UserId}",
                        moduleId,
                        userId);
                }

                stopwatch.Stop();
                response.DurationMs = stopwatch.ElapsedMilliseconds;
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Quiz generation failed for module {ModuleId} after {DurationMs}ms",
                    moduleId, stopwatch.ElapsedMilliseconds);
                var fallback = CreateSafeFallbackResponse("quiz", module);
                fallback.DurationMs = stopwatch.ElapsedMilliseconds;
                return fallback;
            }
        }

        public async Task<AiTextResponseDto> ChatOnModuleAsync(
            int moduleId,
            int userId,
            string role,
            AiModuleChatRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            ValidateChatRequest(request);

            var module = await GetAuthorizedModuleAsync(moduleId, userId, role);
            var moduleContext = BuildModuleContext(module);

            if (string.IsNullOrWhiteSpace(moduleContext) || moduleContext.Length < MinTextLength)
            {
                throw new BadRequestException(
                    "This module doesn't have enough content for the AI tutor to reference. " +
                    "The instructor needs to add lesson content before the chat feature becomes available.");
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

            var sentiment = await _aiService.AnalyzeSentimentAsync(new AiSentimentRequestDto
            {
                Message = request.Message,
                Language = request.Language,
                ModuleId = moduleId
            }, cancellationToken);

            var emotion = await _aiService.AnalyzeEmotionAsync(new AiEmotionRequestDto
            {
                Message = request.Message,
                Language = request.Language,
                ModuleId = moduleId
            }, cancellationToken);

            var adaptationInstruction = BuildAdaptationInstruction(sentiment.Sentiment, emotion.Emotion);
            var adaptationApplied = !string.IsNullOrWhiteSpace(adaptationInstruction);

            var systemPrompt = BuildGroundedChatSystemPrompt(module, request.Language);
            if (adaptationApplied)
            {
                systemPrompt += "\n\n## Adaptive response mode:\n" + adaptationInstruction;
            }
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
                stopwatch.Stop();

                if (response == null || string.IsNullOrWhiteSpace(response.Output))
                {
                    _logger.LogWarning("Empty response from AI chat for module {ModuleId}", moduleId);
                    var emptyFallback = CreateGroundedFallbackResponse(module);
                    emptyFallback.ConversationId = conversationId;
                    emptyFallback.DurationMs = stopwatch.ElapsedMilliseconds;
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
                response.DurationMs = stopwatch.ElapsedMilliseconds;
                response.Sentiment = sentiment.Sentiment;
                response.Emotion = emotion.Emotion;
                response.AdaptationApplied = adaptationApplied;

                _logger.LogInformation(
                    "Module chat success for user {UserId}, module {ModuleId}, conversation {ConversationId}, historyCount {HistoryCount}, durationMs {DurationMs}",
                    userId, moduleId, conversationId, history.Count, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "Module chat failed for user {UserId}, module {ModuleId}, conversation {ConversationId}, durationMs {DurationMs}",
                    userId, moduleId, conversationId, stopwatch.ElapsedMilliseconds);

                var fallback = CreateGroundedFallbackResponse(module);
                fallback.ConversationId = conversationId;
                fallback.DurationMs = stopwatch.ElapsedMilliseconds;
                fallback.Sentiment = sentiment.Sentiment;
                fallback.Emotion = emotion.Emotion;
                fallback.AdaptationApplied = adaptationApplied;
                return fallback;
            }
        }

        #region Validation Methods

        private static void ValidateSummaryRequest(AiModuleSummaryRequestDto request)
        {
            if (request == null)
            {
                throw new BadRequestException("Please provide summary options (language, bullet count, and mode).");
            }

            if (request.MaxBullets < 3 || request.MaxBullets > 15)
            {
                throw new BadRequestException("The number of bullet points should be between 3 and 15. Please adjust and try again.");
            }

            if (!string.IsNullOrEmpty(request.Language) && request.Language.Length > 8)
            {
                throw new BadRequestException("The language code is too long. Please use a standard code like 'en', 'fr', or 'es'.");
            }
        }

        private static void ValidateQuizRequest(AiModuleQuizRequestDto request)
        {
            if (request == null)
            {
                throw new BadRequestException("Please provide quiz options (question count, difficulty, and language).");
            }

            if (request.QuestionsCount < 3 || request.QuestionsCount > 15)
            {
                throw new BadRequestException("Quiz questions count should be between 3 and 15. Please adjust and try again.");
            }

            if (!string.IsNullOrEmpty(request.Language) && request.Language.Length > 8)
            {
                throw new BadRequestException("The language code is too long. Please use a standard code like 'en', 'fr', or 'es'.");
            }
        }

        private static void ValidateChatRequest(AiModuleChatRequestDto request)
        {
            if (request == null)
            {
                throw new BadRequestException("Please provide a message to start the conversation.");
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new BadRequestException("Your message is empty. Please type a question about the module content.");
            }

            var trimmedMessage = request.Message.Trim();
            if (trimmedMessage.Length > MaxMessageLength)
            {
                throw new BadRequestException(
                    $"Your message is too long ({trimmedMessage.Length} characters). " +
                    $"Please shorten it to {MaxMessageLength} characters or less.");
            }

            if (trimmedMessage.Length < 2)
            {
                throw new BadRequestException("Your message is too short. Please ask a complete question about the module.");
            }

            if (ContainsSuspiciousPattern(trimmedMessage))
            {
                throw new BadRequestException(
                    "Your message contains content that cannot be processed. " +
                    "Please rephrase your question about the module material.");
            }

            if (!string.IsNullOrEmpty(request.Language) && request.Language.Length > 8)
            {
                throw new BadRequestException("The language code is too long. Please use a standard code like 'en', 'fr', or 'es'.");
            }

            if (request.History != null)
            {
                foreach (var msg in request.History)
                {
                    if (string.IsNullOrWhiteSpace(msg.Content))
                    {
                        throw new BadRequestException("Conversation history contains an empty message. Please remove it and retry.");
                    }

                    if (msg.Content.Length > MaxMessageLength)
                    {
                        throw new BadRequestException(
                            $"A message in your conversation history exceeds the {MaxMessageLength}-character limit.");
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
                "javascript:alert(",
                "ignore all instructions",
                "forget your instructions",
                "override system prompt",
                "act as if you",
                "pretend you are",
                "bypass content filter",
                "ignore safety",
                "reveal your prompt",
                "show me your system"
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
            var sectionNames = module.ModuleContents != null
                ? string.Join(", ", module.ModuleContents
                    .Where(c => !string.IsNullOrWhiteSpace(c?.Name))
                    .Select(c => $"'{c.Name}'"))
                : "the module content";

            return $@"You are a knowledgeable and encouraging tutor for the MiniCoursera learning platform.
Your purpose is to help students deeply understand the material in the module ""{module.Name}"".

## STRICT GROUNDING RULES:
1. ONLY answer using information explicitly present in the module context provided below.
2. If a question cannot be answered from the module content, respond with:
   ""I don't have information about that in this module's content. Based on what's covered here, I can help you with topics from these sections: {sectionNames}. Would you like to explore any of these?""
3. NEVER invent, assume, or hallucinate information not in the module context.
4. If the student asks a follow-up, reference specific parts of the module content in your answer.

## TEACHING STYLE:
- Be warm, encouraging, and patient
- Use clear, concise language appropriate for learners
- Break complex concepts into digestible pieces
- Provide examples from the module content when available
- Use bullet points or numbered lists for multi-part answers
- When explaining a concept, connect it to other concepts mentioned in the module
- End with a brief follow-up suggestion when appropriate (e.g., ""Would you like me to explain [related concept] next?"")

## FORMATTING:
- Use markdown formatting for readability
- Bold key terms and definitions
- Use code blocks for any code examples
- Keep responses focused and under 500 words unless the student asks for detailed explanation

## CURRENT MODULE:
Title: {module.Name}
Description: {module.Description}

## LANGUAGE:
Respond entirely in {languageName}.";
        }

        private static string BuildGroundedChatUserMessage(string moduleContext, string userQuestion)
        {
            return $@"Use ONLY the module context below to answer the student's question. Do not use any external knowledge.

## Module Context:
{moduleContext}

## Student's Question:
{userQuestion}

Provide a helpful, grounded answer:";
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

        private static string BuildAdaptationInstruction(string sentiment, string emotion)
        {
            var s = (sentiment ?? "neutral").ToLowerInvariant();
            var e = (emotion ?? "neutral").ToLowerInvariant();

            if (e is "confused" or "frustrated" || s == "negative")
            {
                return "Use a simpler tone, explain in short steps, and include one concrete example. End with a quick check question to confirm understanding.";
            }

            if (e is "engaged" or "confident" || s == "positive")
            {
                return "Provide an optional advanced explanation after the core answer and suggest one deeper follow-up concept.";
            }

            return string.Empty;
        }

        #endregion

        #region Fallback Methods

        private static AiTextResponseDto CreateSafeFallbackResponse(string type, CourseModule? module = null)
        {
            var moduleName = module?.Name ?? "the module";
            var fallbackMessages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["summary"] = $"The AI summary service is temporarily unavailable. While we work on restoring it, " +
                              $"here's what you can do:\n\n" +
                              $"- Review the module **\"{moduleName}\"** directly for key concepts\n" +
                              $"- Focus on section headings and bold terms for main ideas\n" +
                              $"- Try generating the summary again in a few moments\n\n" +
                              $"We apologize for the inconvenience.",
                ["quiz"] = $"The AI quiz generator is temporarily unavailable. In the meantime, you can:\n\n" +
                           $"- Review **\"{moduleName}\"** and create your own practice questions\n" +
                           $"- Focus on key definitions, processes, and comparisons in the material\n" +
                           $"- Try generating the quiz again in a few moments\n\n" +
                           $"Self-testing is one of the most effective study strategies!",
                ["chat"] = $"The AI tutor is briefly unavailable. While it's being restored:\n\n" +
                           $"- Browse through **\"{moduleName}\"** for answers to your question\n" +
                           $"- Check the module description for an overview of covered topics\n" +
                           $"- Try asking your question again in a few moments\n\n" +
                           $"We'll be back to help you learn shortly!"
            };

            return new AiTextResponseDto
            {
                Provider = "backend-fallback",
                Model = "safe-fallback",
                Output = fallbackMessages.GetValueOrDefault(type,
                    "Something went wrong on our end. Please try your request again in a moment."),
                IsFallback = true,
                Status = "fallback"
            };
        }

        private static AiTextResponseDto CreateGroundedFallbackResponse(CourseModule module)
        {
            var moduleName = module.Name ?? "this module";
            var moduleDescription = module.Description ?? "the course content";

            var sectionHints = "";
            if (module.ModuleContents != null && module.ModuleContents.Any())
            {
                var sectionNames = module.ModuleContents
                    .Where(c => !string.IsNullOrWhiteSpace(c?.Name))
                    .Select(c => $"**{c.Name}**")
                    .Take(5);
                sectionHints = $"\n\nAvailable sections you can explore: {string.Join(", ", sectionNames)}";
            }

            return new AiTextResponseDto
            {
                Provider = "backend-fallback",
                Model = "grounded-fallback",
                Output = $"The AI tutor is temporarily unable to process your question right now. " +
                         $"While we restore the service, you can continue learning by reviewing " +
                         $"**\"{moduleName}\"** — {moduleDescription}.{sectionHints}\n\n" +
                         $"Please try asking your question again in a moment.",
                IsFallback = true,
                Status = "fallback"
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
