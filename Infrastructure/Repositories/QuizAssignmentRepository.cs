using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class QuizAssignmentRepository : IQuizAssignmentRepository
    {
        private readonly MiniCourseraContext _miniCourseraContext;

        public QuizAssignmentRepository(MiniCourseraContext miniCourseraContext)
        {
            _miniCourseraContext = miniCourseraContext;
        }

        public async Task AddAsync(QuizAssignment entity)
        {
            await _miniCourseraContext.QuizAssignments.AddAsync(entity);
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<QuizAssignment>> GetAllAsync()
        {
            return await _miniCourseraContext.QuizAssignments.ToListAsync();
        }

        public async Task<QuizAssignment?> GetByIdAsync(int id)
        {
            return await _miniCourseraContext.QuizAssignments.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<QuizAssignment?> GetByQuizAndEnrollmentAsync(int quizId, int enrollmentId)
        {
            return await _miniCourseraContext.QuizAssignments
                .FirstOrDefaultAsync(a => a.AiGeneratedQuizId == quizId && a.EnrollmentId == enrollmentId);
        }

        public async Task<List<QuizAssignment>> GetByCourseIdAsync(int courseId)
        {
            return await _miniCourseraContext.QuizAssignments
                .Include(a => a.AiGeneratedQuiz)
                    .ThenInclude(q => q.Module)
                .Include(a => a.Enrollment)
                    .ThenInclude(e => e!.Student)
                .Where(a => a.AiGeneratedQuiz != null && a.AiGeneratedQuiz.Module != null && a.AiGeneratedQuiz.Module.CourseId == courseId)
                .ToListAsync();
        }

        public Task UpdateAsync(QuizAssignment entity)
        {
            _miniCourseraContext.QuizAssignments.Update(entity);
            return Task.CompletedTask;
        }
    }
}
