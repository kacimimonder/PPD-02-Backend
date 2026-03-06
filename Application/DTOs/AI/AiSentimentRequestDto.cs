using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AI
{
    public class AiSentimentRequestDto
    {
        [Required(ErrorMessage = "Message is required")]
        [MinLength(1, ErrorMessage = "Message cannot be empty")]
        [MaxLength(4000, ErrorMessage = "Message cannot exceed 4000 characters")]
        public string Message { get; set; } = string.Empty;

        [MaxLength(8, ErrorMessage = "Language code cannot exceed 8 characters")]
        public string Language { get; set; } = "en";

        public int? ModuleId { get; set; }
    }
}
