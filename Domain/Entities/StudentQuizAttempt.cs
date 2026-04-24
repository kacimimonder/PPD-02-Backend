using System;

namespace Domain.Entities
{
    public class StudentQuizAttempt
    {
        public int Id { get; set; }

        public int AiGeneratedQuizId { get; set; }
        public AiGeneratedQuiz? AiGeneratedQuiz { get; set; }

        public int EnrollmentId { get; set; }
        public Enrollment? Enrollment { get; set; }

        public int? QuizAssignmentId { get; set; }
        public QuizAssignment? QuizAssignment { get; set; }

        public int AttemptNumber { get; set; } = 1;
        public string StudentResponses { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsCompleted { get; set; } = true;
        public DateTime? CompletedAt { get; set; }
        public int DurationSeconds { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
