using System.Text;
using Application.DTOs.AI;
using Application.Exceptions;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class AiModuleService
    {
        private const int MaxModuleContextChars = 12000;
        private const int MaxHistoryMessages = 12;

        private readonly ICourseModuleRepository _courseModuleRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly AiService _aiService;

        public AiModuleService(
            ICourseModuleRepository courseModuleRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            AiService aiService)
        {
            _courseModuleRepository = courseModuleRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _aiService = aiService;
        }

        public async Task<AiTextResponseDto> GenerateModuleSummaryAsync(
            int moduleId,
            int userId,
            string role,
            AiModuleSummaryRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var module = await GetAuthorizedModuleAsync(moduleId, userId, role);
            var rawContext = BuildModuleContext(module);

            if (string.IsNullOrWhiteSpace(rawContext))
            {
                throw new BadRequestException("This module has no text content yet to summarize.");
            }

            var content = "Context: The following is a single course module. Summarize only this module for a student.\n\n" + rawContext;

            var summaryRequest = new AiSummaryRequestDto
            {
                Text = content,
                MaxBullets = request.MaxBullets,
                Language = request.Language
            };

            return await _aiService.SummarizeAsync(summaryRequest, cancellationToken);
        }

        public async Task<AiTextResponseDto> GenerateModuleQuizAsync(
            int moduleId,
            int userId,
            string role,
            AiModuleQuizRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var module = await GetAuthorizedModuleAsync(moduleId, userId, role);
            var rawContext = BuildModuleContext(module);

            if (string.IsNullOrWhiteSpace(rawContext))
            {
                throw new BadRequestException("This module has no text content yet to generate a quiz.");
            }

            var content = "Context: The following is a single course module. Generate a quiz based only on this module.\n\n" + rawContext;

            var quizRequest = new AiQuizRequestDto
            {
                Text = content,
                QuestionsCount = request.QuestionsCount,
                Language = request.Language
            };

            return await _aiService.GenerateQuizAsync(quizRequest, cancellationToken);
        }

        public async Task<AiTextResponseDto> ChatOnModuleAsync(
            int moduleId,
            int userId,
            string role,
            AiModuleChatRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new BadRequestException("Message cannot be empty.");
            }

            var module = await GetAuthorizedModuleAsync(moduleId, userId, role);
            var moduleContext = BuildModuleContext(module);

            if (string.IsNullOrWhiteSpace(moduleContext))
            {
                throw new BadRequestException("This module has no text content yet for grounded chat.");
            }

            moduleContext = TrimToMaxChars(moduleContext, MaxModuleContextChars);

            var history = request.History ?? new List<AiChatMessageDto>();
            if (history.Count > MaxHistoryMessages)
            {
                history = history.Skip(history.Count - MaxHistoryMessages).ToList();
            }

            var groundedMessage =
                "You are a module-grounded tutor for MiniCoursera. " +
                "Answer the student's question using the module context below. " +
                "If the context is insufficient, say so clearly and suggest what part to review.\n\n" +
                $"Module Context:\n{moduleContext}\n\n" +
                $"Student Question:\n{request.Message}";

            var chatRequest = new AiChatRequestDto
            {
                Message = groundedMessage,
                Language = request.Language,
                History = history
            };

            try
            {
                return await _aiService.ChatAsync(chatRequest, cancellationToken);
            }
            catch (Exception)
            {
                return new AiTextResponseDto
                {
                    Provider = "backend-fallback",
                    Model = "n/a",
                    Output = "AI chat is temporarily unavailable. Please try again in a moment. For now, review the module title, description, and the first content section to continue learning."
                };
            }
        }

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

        private static string TrimToMaxChars(string value, int maxChars)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxChars)
            {
                return value;
            }

            return value[..maxChars];
        }
    }
}
