using Application.DTOs.AI;
using Application.DTOs.Quiz;
using Application.Exceptions;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class QuizProgressService
    {
        private readonly IAiGeneratedQuizRepository _aiGeneratedQuizRepository;
        private readonly IQuizAssignmentRepository _quizAssignmentRepository;
        private readonly IStudentQuizAttemptRepository _studentQuizAttemptRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseModuleRepository _courseModuleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public QuizProgressService(
            IAiGeneratedQuizRepository aiGeneratedQuizRepository,
            IQuizAssignmentRepository quizAssignmentRepository,
            IStudentQuizAttemptRepository studentQuizAttemptRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseModuleRepository courseModuleRepository,
            ICourseRepository courseRepository,
            IUnitOfWork unitOfWork)
        {
            _aiGeneratedQuizRepository = aiGeneratedQuizRepository;
            _quizAssignmentRepository = quizAssignmentRepository;
            _studentQuizAttemptRepository = studentQuizAttemptRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseModuleRepository = courseModuleRepository;
            _courseRepository = courseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AiGeneratedQuiz> PersistGeneratedQuizAsync(
            int moduleId,
            int userId,
            string role,
            AiModuleQuizRequestDto request,
            string output)
        {
            var module = await _courseModuleRepository.GetByIdWithContentsAsync(moduleId);
            if (module == null)
            {
                throw new NotFoundException($"Module with ID {moduleId} not found.");
            }

            int? generatedForEnrollmentId = null;
            if (role == "Student")
            {
                var enrollment = await _enrollmentRepository.GetEnrollmentByCourseIdAndStudentId(module.CourseId, userId);
                if (enrollment != null)
                {
                    generatedForEnrollmentId = enrollment.Id;
                }
            }

            var entity = new AiGeneratedQuiz
            {
                ModuleId = moduleId,
                GeneratedByUserId = userId,
                GeneratedForEnrollmentId = generatedForEnrollmentId,
                Output = output,
                Difficulty = (int)request.Difficulty,
                QuestionsCount = request.QuestionsCount,
                Language = request.Language,
                IncludeExplanations = request.IncludeExplanations,
                GenerationSource = role == "Instructor" ? "Instructor" : "Student"
            };

            await _aiGeneratedQuizRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task<int> AssignQuizToStudentsAsync(int instructorId, QuizAssignmentCreateDto request)
        {
            var quiz = await _aiGeneratedQuizRepository.GetByIdWithModuleAsync(request.QuizId)
                ?? throw new NotFoundException($"Quiz with ID {request.QuizId} was not found.");

            var module = quiz.Module ?? throw new NotFoundException("Quiz module context is missing.");
            var isOwner = await _courseRepository.IsCourseCreatedByInstructor(instructorId, module.CourseId);
            if (!isOwner)
            {
                throw new ForbiddenException("You can only assign quizzes for your own courses.");
            }

            if (request.EnrollmentIds == null || request.EnrollmentIds.Count == 0)
            {
                throw new BadRequestException("Please provide at least one enrollment ID.");
            }

            var createdCount = 0;
            foreach (var enrollmentId in request.EnrollmentIds.Distinct())
            {
                var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
                if (enrollment == null || enrollment.CourseId != module.CourseId)
                {
                    throw new BadRequestException($"Enrollment {enrollmentId} does not belong to this quiz course.");
                }

                var exists = await _quizAssignmentRepository.GetByQuizAndEnrollmentAsync(request.QuizId, enrollmentId);
                if (exists != null)
                {
                    continue;
                }

                var assignment = new QuizAssignment
                {
                    AiGeneratedQuizId = request.QuizId,
                    EnrollmentId = enrollmentId,
                    AssignedByInstructorId = instructorId,
                    DueAt = request.DueAt
                };

                await _quizAssignmentRepository.AddAsync(assignment);
                createdCount++;
            }

            await _unitOfWork.SaveChangesAsync();
            return createdCount;
        }

        public async Task<StudentQuizAttemptReadDto> CreateAttemptAsync(int studentId, StudentQuizAttemptCreateDto request)
        {
            var quiz = await _aiGeneratedQuizRepository.GetByIdWithModuleAsync(request.QuizId)
                ?? throw new NotFoundException($"Quiz with ID {request.QuizId} was not found.");

            var module = quiz.Module ?? throw new NotFoundException("Quiz module context is missing.");
            var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId)
                ?? throw new NotFoundException($"Enrollment {request.EnrollmentId} was not found.");

            if (enrollment.StudentId != studentId)
            {
                throw new ForbiddenException("You can only submit attempts for your own enrollments.");
            }

            if (enrollment.CourseId != module.CourseId)
            {
                throw new BadRequestException("Enrollment does not belong to the quiz course.");
            }

            if (request.QuizAssignmentId.HasValue)
            {
                var assignment = await _quizAssignmentRepository.GetByIdAsync(request.QuizAssignmentId.Value)
                    ?? throw new NotFoundException($"Assignment {request.QuizAssignmentId.Value} was not found.");
                if (assignment.EnrollmentId != request.EnrollmentId || assignment.AiGeneratedQuizId != request.QuizId)
                {
                    throw new BadRequestException("Assignment does not match this enrollment and quiz.");
                }
            }

            var attemptNumber = await _studentQuizAttemptRepository.GetAttemptsCountAsync(request.QuizId, request.EnrollmentId) + 1;
            var attempt = new StudentQuizAttempt
            {
                AiGeneratedQuizId = request.QuizId,
                EnrollmentId = request.EnrollmentId,
                QuizAssignmentId = request.QuizAssignmentId,
                AttemptNumber = attemptNumber,
                StudentResponses = request.StudentResponses,
                Score = request.Score,
                CorrectAnswers = request.CorrectAnswers,
                TotalQuestions = request.TotalQuestions,
                IsCompleted = request.IsCompleted,
                CompletedAt = request.IsCompleted ? DateTime.UtcNow : null,
                DurationSeconds = request.DurationSeconds
            };

            await _studentQuizAttemptRepository.AddAsync(attempt);
            await _unitOfWork.SaveChangesAsync();

            return new StudentQuizAttemptReadDto
            {
                AttemptId = attempt.Id,
                QuizId = attempt.AiGeneratedQuizId,
                EnrollmentId = attempt.EnrollmentId,
                AttemptNumber = attempt.AttemptNumber,
                StudentResponses = attempt.StudentResponses,
                Score = attempt.Score,
                CorrectAnswers = attempt.CorrectAnswers,
                TotalQuestions = attempt.TotalQuestions,
                IsCompleted = attempt.IsCompleted,
                CompletedAt = attempt.CompletedAt
            };
        }

        public async Task<InstructorCourseQuizProgressDto> GetInstructorCourseProgressAsync(int instructorId, int courseId)
        {
            var isOwner = await _courseRepository.IsCourseCreatedByInstructor(instructorId, courseId);
            if (!isOwner)
            {
                throw new ForbiddenException("You can only view progress for courses you authored.");
            }

            var course = await _courseRepository.GetByIdAsync(courseId)
                ?? throw new NotFoundException($"Course with ID {courseId} was not found.");

            var enrollments = await _enrollmentRepository.GetCourseEnrollmentsWithStudentAsync(courseId);
            var quizzes = await _aiGeneratedQuizRepository.GetByCourseIdAsync(courseId);
            var assignments = await _quizAssignmentRepository.GetByCourseIdAsync(courseId);
            var attempts = await _studentQuizAttemptRepository.GetByCourseIdAsync(courseId);

            var response = new InstructorCourseQuizProgressDto
            {
                CourseId = course.Id,
                CourseTitle = course.Title
            };

            foreach (var enrollment in enrollments)
            {
                var student = enrollment.Student;
                if (student == null)
                {
                    continue;
                }

                var generatedForStudent = quizzes
                    .Where(q => q.GeneratedForEnrollmentId == enrollment.Id)
                    .Select(q => q.Id)
                    .ToHashSet();

                foreach (var assignment in assignments.Where(a => a.EnrollmentId == enrollment.Id))
                {
                    generatedForStudent.Add(assignment.AiGeneratedQuizId);
                }

                var studentProgress = new StudentQuizProgressDto
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    StudentEmail = student.Email,
                    EnrollmentId = enrollment.Id,
                    IsCourseCompleted = enrollment.IsCompleted,
                    CompletedContentItems = enrollment.enrollmentProgresses?.Count ?? 0
                };

                foreach (var quizId in generatedForStudent)
                {
                    var quiz = quizzes.FirstOrDefault(q => q.Id == quizId);
                    if (quiz == null)
                    {
                        continue;
                    }

                    var assignment = assignments.FirstOrDefault(a => a.EnrollmentId == enrollment.Id && a.AiGeneratedQuizId == quizId);
                    var quizAttempts = attempts
                        .Where(a => a.EnrollmentId == enrollment.Id && a.AiGeneratedQuizId == quizId)
                        .OrderByDescending(a => a.CompletedAt ?? a.CreatedAt)
                        .ToList();
                    var latestAttempt = quizAttempts.FirstOrDefault();

                    studentProgress.Quizzes.Add(new StudentQuizProgressItemDto
                    {
                        QuizId = quiz.Id,
                        ModuleId = quiz.ModuleId,
                        ModuleName = quiz.Module?.Name ?? string.Empty,
                        GenerationSource = quiz.GenerationSource,
                        IsAssigned = assignment != null,
                        AssignmentId = assignment?.Id,
                        DueAt = assignment?.DueAt,
                        AttemptsCount = quizAttempts.Count,
                        LatestScore = latestAttempt?.Score,
                        LatestCompleted = latestAttempt?.IsCompleted ?? false,
                        LatestCompletedAt = latestAttempt?.CompletedAt,
                        LatestStudentResponses = latestAttempt?.StudentResponses ?? string.Empty
                    });
                }

                response.Students.Add(studentProgress);
            }

            return response;
        }
    }
}
