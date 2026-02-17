using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces.Utilities;

namespace Application.Services
{
    public class VideoService
    {
        private readonly IVideoService _videoService;
        public VideoService(IVideoService videoStorageService)
        {
            _videoService = videoStorageService;
        }

        public async Task<string> UploadVideoAsync(Stream fileStream, string fileName)
        {
            return await _videoService.UploadVideoAsync(fileStream, fileName);
        }

    }
}
