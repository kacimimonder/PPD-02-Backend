using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AI
{
    public class AiCourseRecommendationsRequestDto
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

        public List<AiCourseCandidateDto> Courses { get; set; } = new();
    }

    public class AiCourseCandidateDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class AiCourseRecommendationItemDto
    {
        public int CourseId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public double MatchScore { get; set; }
    }

    public class AiCourseRecommendationsResponseDto
    {
        public string Summary { get; set; } = string.Empty;
        public List<AiCourseRecommendationItemDto> Recommendations { get; set; } = new();
        public string Provider { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public bool IsFallback { get; set; }
        public string Status { get; set; } = "success";
    }

    public class AiRecommendationsProfileDto
    {
        public string Ambitions { get; set; } = string.Empty;
        public string Interests { get; set; } = string.Empty;
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class AiRecommendedCourseCardDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string InstructorImageUrl { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public double MatchScore { get; set; }
    }

    public class AiCourseRecommendationsResultDto
    {
        public string Summary { get; set; } = string.Empty;
        public List<AiRecommendedCourseCardDto> Courses { get; set; } = new();
        public string Provider { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public bool IsFallback { get; set; }
        public string Status { get; set; } = "success";
    }
}
