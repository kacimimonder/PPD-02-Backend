using Application.DTOs.ModuleContent;

namespace API.DTO
{
    public class ModuleContentDTOUpdate
    {
        public ModuleContentUpdateDTO ModuleContentUpdateDTO { get; set; } = default!;
        public IFormFile? videoFile { get; set; }
    }

}
