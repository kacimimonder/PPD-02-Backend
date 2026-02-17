using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Domain.Interfaces.Utilities;

namespace Infrastructure.Utilities
{
    public class CloudinaryVideoService:IVideoService
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryVideoService(IConfiguration config)
        {
            var cloudName = config["Cloudinary:CloudName"];
            var apiKey = config["Cloudinary:ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"];

            Account account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        private string ExtractPublicIdFromUrl(string videoUrl)
        {
            var uri = new Uri(videoUrl);
            var segments = uri.AbsolutePath.Split('/');
            var filename = segments.Last(); // e.g. "abc123_xyz.mp4"
            var folder = segments[^2];      // e.g. "course_videos"
            var publicId = Path.Combine(folder, Path.GetFileNameWithoutExtension(filename)).Replace('\\', '/');
            return publicId;
        }

        public async Task<string> UploadVideoAsync(Stream fileStream, string fileName)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";

            var uploadParams = new VideoUploadParams()
            {
                File = new FileDescription(uniqueFileName, fileStream),
                Folder = "course_videos"
            };
            try
            {
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                return uploadResult.SecureUrl.ToString();
            }
            catch
            {
                return null;
            }
        }
        
        public async Task<bool> DeleteVideoAsync(string? url)
        {
            if (url == null || url == "") return false;
            string publicId = ExtractPublicIdFromUrl(url);
            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Video
            };

            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            if (deletionResult.Result != "ok")
            {
                return false;
            }
            return true;
        }

        public string? GetStreamingUrl(string videoUrl, bool signUrl = false)
        {
            try
            {
                if (string.IsNullOrEmpty(videoUrl)) return null;

                string publicId = ExtractPublicIdFromUrl(videoUrl);

                var transformation = new Transformation().StreamingProfile("full_hd");

                var url = _cloudinary.Api.UrlVideoUp
                    .Transform(transformation)
                    .Secure(true)
                    .Format("m3u8");

                if (signUrl)
                {
                    url = url.Signed(true); // signs the URL
                }

                return url.BuildUrl(publicId);
            }
            catch
            {
                return null;
            }

        }


    }
}
