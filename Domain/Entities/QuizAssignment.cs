using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class QuizAssignment
    {
        public int Id { get; set; }

        public int AiGeneratedQuizId { get; set; }
        public AiGeneratedQuiz? AiGeneratedQuiz { get; set; }

        public int EnrollmentId { get; set; }
        public Enrollment? Enrollment { get; set; }

        public int AssignedByInstructorId { get; set; }
        public User? AssignedByInstructor { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueAt { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<StudentQuizAttempt> Attempts { get; set; } = new List<StudentQuizAttempt>();
    }
}
