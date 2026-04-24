namespace Application.DTOs.Quiz
{
    public class QuizAssignmentCreateDto
    {
        public int QuizId { get; set; }
        public List<int> EnrollmentIds { get; set; } = new List<int>();
        public DateTime? DueAt { get; set; }
    }
}
