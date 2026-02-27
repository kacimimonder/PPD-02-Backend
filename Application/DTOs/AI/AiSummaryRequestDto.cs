using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AI
{
    public class AiSummaryRequestDto
    {
        [Required(ErrorMessage = "Text content is required")]
        [MinLength(10, ErrorMessage = "Text must be at least 10 characters")]
        [MaxLength(25000, ErrorMessage = "Text cannot exceed 25000 characters")]
        public string Text { get; set; } = string.Empty;

        [Range(3, 15, ErrorMessage = "Max bullets must be between 3 and 15")]
        public int MaxBullets { get; set; } = 5;

        [MaxLength(8, ErrorMessage = "Language code cannot exceed 8 characters")]
        public string Language { get; set; } = "en";

        /// <summary>
        /// Summary mode: Short (bullet points) or Detailed (paragraphs)
        /// </summary>
        public SummaryMode Mode { get; set; } = SummaryMode.Short;
    }

    public class AiModuleSummaryRequestDto
    {
        [Range(3, 15, ErrorMessage = "Max bullets must be between 3 and 15")]
        public int MaxBullets { get; set; } = 5;

        [MaxLength(8, ErrorMessage = "Language code cannot exceed 8 characters")]
        public string Language { get; set; } = "en";

        /// <summary>
        /// Summary mode: Short (bullet points) or Detailed (paragraphs)
        /// </summary>
        public SummaryMode Mode { get; set; } = SummaryMode.Short;
    }
}
