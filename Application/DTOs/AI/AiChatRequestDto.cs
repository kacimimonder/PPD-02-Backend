using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AI
{
    public class AiChatRequestDto
    {
        [Required]
        [MaxLength(4000)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(8)]
        public string Language { get; set; } = "en";

        public List<AiChatMessageDto> History { get; set; } = new();
    }

    public class AiChatMessageDto
    {
        [Required]
        [MaxLength(16)]
        public string Role { get; set; } = "user";

        [Required]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;
    }
}
