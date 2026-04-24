using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IAiGeneratedQuizRepository : IBaseRepository<AiGeneratedQuiz>
    {
        Task<AiGeneratedQuiz?> GetByIdWithModuleAsync(int quizId);
        Task<List<AiGeneratedQuiz>> GetByCourseIdAsync(int courseId);
    }
}
