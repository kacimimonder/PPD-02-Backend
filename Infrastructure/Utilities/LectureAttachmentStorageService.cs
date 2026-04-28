using Domain.Interfaces.Utilities;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Utilities
{
    public class LectureAttachmentStorageService : ILectureAttachmentStorageService
    {
        private readonly string _storageRoot;

        public LectureAttachmentStorageService(IHostEnvironment environment)
        {
            var webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");
            if (!Directory.Exists(webRoot))
            {
                webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            }

            _storageRoot = Path.Combine(webRoot, "uploads", "lecture-attachments");
            Directory.CreateDirectory(_storageRoot);
        }

        public async Task<string> SaveAttachmentAsync(byte[] fileBytes, string originalFileName, string contentType)
        {
            var extension = Path.GetExtension(originalFileName);
            var safeName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(_storageRoot, safeName);
            await File.WriteAllBytesAsync(fullPath, fileBytes);
            return $"/uploads/lecture-attachments/{safeName}";
        }

        public Task<bool> DeleteAttachmentAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileUrl))
                {
                    return Task.FromResult(false);
                }

                var relativePath = fileUrl.Replace("/uploads/lecture-attachments/", string.Empty).TrimStart('/').TrimStart('\\');
                var fullPath = Path.Combine(_storageRoot, relativePath);
                if (!File.Exists(fullPath))
                {
                    return Task.FromResult(false);
                }

                File.Delete(fullPath);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}
