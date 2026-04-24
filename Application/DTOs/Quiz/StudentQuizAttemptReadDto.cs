namespace Application.DTOs.Quiz
{
    public class StudentQuizAttemptReadDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public int EnrollmentId { get; set; }
        public int AttemptNumber { get; set; }
        public string StudentResponses { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
