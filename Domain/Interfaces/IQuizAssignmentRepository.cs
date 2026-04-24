using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IQuizAssignmentRepository : IBaseRepository<QuizAssignment>
    {
        Task<QuizAssignment?> GetByQuizAndEnrollmentAsync(int quizId, int enrollmentId);
        Task<List<QuizAssignment>> GetByCourseIdAsync(int courseId);
    }
}
