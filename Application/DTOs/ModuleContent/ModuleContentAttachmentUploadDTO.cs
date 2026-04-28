namespace Application.DTOs.ModuleContent
{
    public class ModuleContentAttachmentUploadDTO
    {
        public string FileName { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public byte[] FileBytes { get; set; } = Array.Empty<byte>();
        public string AttachmentType { get; set; } = default!;
    }
}
