using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IStudentQuizAttemptRepository : IBaseRepository<StudentQuizAttempt>
    {
        Task<int> GetAttemptsCountAsync(int quizId, int enrollmentId);
        Task<StudentQuizAttempt?> GetLatestAttemptAsync(int quizId, int enrollmentId);
        Task<List<StudentQuizAttempt>> GetByCourseIdAsync(int courseId);
    }
}
