using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AI
{
    public class AiRecommendCoursesRequestDto
    {
        [Required]
        [MaxLength(1200)]
        public string Ambitions { get; set; } = string.Empty;

        [Required]
        [MaxLength(1200)]
        public string Interests { get; set; } = string.Empty;

        [Range(1, 8)]
        public int MaxRecommendations { get; set; } = 4;

        [MaxLength(8)]
        public string Language { get; set; } = "en";
    }
}
