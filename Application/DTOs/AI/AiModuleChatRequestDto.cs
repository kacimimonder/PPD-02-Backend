using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AI
{
    public class AiModuleChatRequestDto
    {
        [Required]
        [MaxLength(4000)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(64)]
        public string? ConversationId { get; set; }

        public bool UseServerMemory { get; set; } = true;

        [MaxLength(8)]
        public string Language { get; set; } = "en";

        public List<AiChatMessageDto> History { get; set; } = new();
    }
}
