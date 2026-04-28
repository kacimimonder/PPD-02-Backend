using Application.DTOs.ModuleContent;

namespace API.DTO
{
    public class ModuleContentDTOUpdate
    {
        public ModuleContentUpdateDTO ModuleContentUpdateDTO { get; set; } = default!;
        public IFormFile? videoFile { get; set; }
        public List<IFormFile>? imageFiles { get; set; }
        public List<IFormFile>? pdfFiles { get; set; }
        public List<int>? DeleteAttachmentIds { get; set; }
    }

}
