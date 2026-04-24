using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AiGeneratedQuizRepository : IAiGeneratedQuizRepository
    {
        private readonly MiniCourseraContext _miniCourseraContext;

        public AiGeneratedQuizRepository(MiniCourseraContext miniCourseraContext)
        {
            _miniCourseraContext = miniCourseraContext;
        }

        public async Task AddAsync(AiGeneratedQuiz entity)
        {
            await _miniCourseraContext.AiGeneratedQuizzes.AddAsync(entity);
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<AiGeneratedQuiz>> GetAllAsync()
        {
            return await _miniCourseraContext.AiGeneratedQuizzes.ToListAsync();
        }

        public async Task<AiGeneratedQuiz?> GetByIdAsync(int id)
        {
            return await _miniCourseraContext.AiGeneratedQuizzes.FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<AiGeneratedQuiz?> GetByIdWithModuleAsync(int quizId)
        {
            return await _miniCourseraContext.AiGeneratedQuizzes
                .Include(q => q.Module)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task<List<AiGeneratedQuiz>> GetByCourseIdAsync(int courseId)
        {
            return await _miniCourseraContext.AiGeneratedQuizzes
                .Include(q => q.Module)
                .Where(q => q.Module != null && q.Module.CourseId == courseId)
                .ToListAsync();
        }

        public Task UpdateAsync(AiGeneratedQuiz entity)
        {
            _miniCourseraContext.AiGeneratedQuizzes.Update(entity);
            return Task.CompletedTask;
        }
    }
}
