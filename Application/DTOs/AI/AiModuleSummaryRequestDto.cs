using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AI
{
    public class AiModuleSummaryRequestDto
    {
        [Range(3, 12)]
        public int MaxBullets { get; set; } = 5;

        [MaxLength(8)]
        public string Language { get; set; } = "en";
    }
}
