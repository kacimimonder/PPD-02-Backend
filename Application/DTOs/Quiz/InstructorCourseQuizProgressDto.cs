namespace Application.DTOs.Quiz
{
    public class InstructorCourseQuizProgressDto
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public List<StudentQuizProgressDto> Students { get; set; } = new List<StudentQuizProgressDto>();
    }

    public class StudentQuizProgressDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public int EnrollmentId { get; set; }
        public bool IsCourseCompleted { get; set; }
        public int CompletedContentItems { get; set; }
        public List<StudentQuizProgressItemDto> Quizzes { get; set; } = new List<StudentQuizProgressItemDto>();
    }

    public class StudentQuizProgressItemDto
    {
        public int QuizId { get; set; }
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string GenerationSource { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
        public int? AssignmentId { get; set; }
        public DateTime? DueAt { get; set; }
        public int AttemptsCount { get; set; }
        public decimal? LatestScore { get; set; }
        public bool LatestCompleted { get; set; }
        public DateTime? LatestCompletedAt { get; set; }
        public string LatestStudentResponses { get; set; } = string.Empty;
    }
}
