using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AI
{
    public class AiChatRequestDto
    {
        [Required(ErrorMessage = "Message is required")]
        [MinLength(1, ErrorMessage = "Message cannot be empty")]
        [MaxLength(4000, ErrorMessage = "Message cannot exceed 4000 characters")]
        public string Message { get; set; } = string.Empty;

        [MaxLength(8, ErrorMessage = "Language code cannot exceed 8 characters")]
        public string Language { get; set; } = "en";

        public List<AiChatMessageDto> History { get; set; } = new();

        /// <summary>
        /// Optional context for grounded chat (module ID, course info, etc.)
        /// </summary>
        [MaxLength(10000, ErrorMessage = "Context cannot exceed 10000 characters")]
        public string? Context { get; set; }

        /// <summary>
        /// Enable strict grounded mode - only answer from provided context
        /// </summary>
        public bool StrictGrounded { get; set; } = false;
    }

    public class AiChatMessageDto
    {
        [Required(ErrorMessage = "Role is required")]
        [MaxLength(16, ErrorMessage = "Role cannot exceed 16 characters")]
        public string Role { get; set; } = "user";

        [Required(ErrorMessage = "Content is required")]
        [MinLength(1, ErrorMessage = "Content cannot be empty")]
        [MaxLength(4000, ErrorMessage = "Content cannot exceed 4000 characters")]
        public string Content { get; set; } = string.Empty;
    }
}
