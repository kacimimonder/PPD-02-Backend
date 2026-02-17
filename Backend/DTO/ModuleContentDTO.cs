using Application.DTOs.ModuleContent;

namespace API.DTO
{
    public class ModuleContentDTO
    {
        public ModuleContentCreateDTO moduleContentCreateDTO { get; set; } = default!;
        public IFormFile? videoFile { get; set; }

    }
}
