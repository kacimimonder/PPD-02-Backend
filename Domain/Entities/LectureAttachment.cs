using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class LectureAttachment
    {
        public int Id { get; set; }
        public int ModuleContentId { get; set; }
        public ModuleContent? ModuleContent { get; set; }
        public string FileName { get; set; } = default!;
        public string FileUrl { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public string AttachmentType { get; set; } = default!;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
