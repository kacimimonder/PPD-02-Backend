using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Utilities
{
    public interface IVideoService
    {
        Task<string> UploadVideoAsync(Stream fileStream , string fileName);
        Task<bool> DeleteVideoAsync(string url);
        string? GetStreamingUrl(string videoUrl, bool signUrl = false);
    }
}
