using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.DTOs.AI
{
    /// <summary>
    /// Difficulty level for quiz generation
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum QuizDifficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    /// <summary>
    /// Summary mode - short (bullet points) or detailed (paragraphs)
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SummaryMode
    {
        Short = 0,
        Detailed = 1
    }

    public class AiQuizRequestDto
    {
        [Required(ErrorMessage = "Text content is required")]
        [MinLength(30, ErrorMessage = "Text must be at least 30 characters")]
        [MaxLength(25000, ErrorMessage = "Text cannot exceed 25000 characters")]
        public string Text { get; set; } = string.Empty;

        [Range(3, 15, ErrorMessage = "Questions count must be between 3 and 15")]
        public int QuestionsCount { get; set; } = 5;

        [MaxLength(8, ErrorMessage = "Language code cannot exceed 8 characters")]
        public string Language { get; set; } = "en";

        /// <summary>
        /// Difficulty level: Easy, Medium, or Hard
        /// </summary>
        public QuizDifficulty Difficulty { get; set; } = QuizDifficulty.Medium;

        /// <summary>
        /// Include explanations for answers
        /// </summary>
        public bool IncludeExplanations { get; set; } = true;
    }

    public class AiModuleQuizRequestDto
    {
        [Range(3, 15, ErrorMessage = "Questions count must be between 3 and 15")]
        public int QuestionsCount { get; set; } = 5;

        [MaxLength(8, ErrorMessage = "Language code cannot exceed 8 characters")]
        public string Language { get; set; } = "en";

        /// <summary>
        /// Difficulty level: Easy, Medium, or Hard
        /// </summary>
        public QuizDifficulty Difficulty { get; set; } = QuizDifficulty.Medium;

        /// <summary>
        /// Include explanations for answers
        /// </summary>
        public bool IncludeExplanations { get; set; } = true;
    }
}
