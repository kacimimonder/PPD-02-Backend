using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AI
{
    public class AiModuleQuizRequestDto
    {
        [Range(3, 10)]
        public int QuestionsCount { get; set; } = 5;

        [MaxLength(8)]
        public string Language { get; set; } = "en";
    }
}
