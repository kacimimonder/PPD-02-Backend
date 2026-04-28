namespace Application.DTOs.ModuleContent
{
    public class ModuleContentAttachmentReadDTO
    {
        public int Id { get; set; }
        public string FileName { get; set; } = default!;
        public string FileUrl { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public string AttachmentType { get; set; } = default!;
    }
}
