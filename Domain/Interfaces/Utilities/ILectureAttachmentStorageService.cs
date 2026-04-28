using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Utilities
{
    public interface ILectureAttachmentStorageService
    {
        Task<string> SaveAttachmentAsync(byte[] fileBytes, string originalFileName, string contentType);
        Task<bool> DeleteAttachmentAsync(string fileUrl);
    }
}
