namespace Application.DTOs.Quiz
{
    public class StudentQuizAttemptCreateDto
    {
        public int QuizId { get; set; }
        public int EnrollmentId { get; set; }
        public int? QuizAssignmentId { get; set; }
        public string StudentResponses { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsCompleted { get; set; } = true;
        public int DurationSeconds { get; set; }
    }
}
