using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class StudentQuizAttemptRepository : IStudentQuizAttemptRepository
    {
        private readonly MiniCourseraContext _miniCourseraContext;

        public StudentQuizAttemptRepository(MiniCourseraContext miniCourseraContext)
        {
            _miniCourseraContext = miniCourseraContext;
        }

        public async Task AddAsync(StudentQuizAttempt entity)
        {
            await _miniCourseraContext.StudentQuizAttempts.AddAsync(entity);
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<StudentQuizAttempt>> GetAllAsync()
        {
            return await _miniCourseraContext.StudentQuizAttempts.ToListAsync();
        }

        public async Task<StudentQuizAttempt?> GetByIdAsync(int id)
        {
            return await _miniCourseraContext.StudentQuizAttempts.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<int> GetAttemptsCountAsync(int quizId, int enrollmentId)
        {
            return await _miniCourseraContext.StudentQuizAttempts
                .CountAsync(a => a.AiGeneratedQuizId == quizId && a.EnrollmentId == enrollmentId);
        }

        public async Task<StudentQuizAttempt?> GetLatestAttemptAsync(int quizId, int enrollmentId)
        {
            return await _miniCourseraContext.StudentQuizAttempts
                .Where(a => a.AiGeneratedQuizId == quizId && a.EnrollmentId == enrollmentId)
                .OrderByDescending(a => a.CompletedAt ?? a.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<StudentQuizAttempt>> GetByCourseIdAsync(int courseId)
        {
            return await _miniCourseraContext.StudentQuizAttempts
                .Include(a => a.Enrollment)
                    .ThenInclude(e => e!.Student)
                .Include(a => a.AiGeneratedQuiz)
                    .ThenInclude(q => q.Module)
                .Where(a => a.AiGeneratedQuiz != null && a.AiGeneratedQuiz.Module != null && a.AiGeneratedQuiz.Module.CourseId == courseId)
                .ToListAsync();
        }

        public Task UpdateAsync(StudentQuizAttempt entity)
        {
            _miniCourseraContext.StudentQuizAttempts.Update(entity);
            return Task.CompletedTask;
        }
    }
}
