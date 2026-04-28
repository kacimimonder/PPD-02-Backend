using Application.DTOs.ModuleContent;

namespace API.DTO
{
    public class ModuleContentDTO
    {
        public ModuleContentCreateDTO moduleContentCreateDTO { get; set; } = default!;
        public IFormFile? videoFile { get; set; }
        public List<IFormFile>? imageFiles { get; set; }
        public List<IFormFile>? pdfFiles { get; set; }

    }
}
