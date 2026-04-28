using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Domain.Interfaces.Utilities;

namespace Infrastructure.Utilities
{
    public class CloudinaryVideoService:IVideoService
    {
        private readonly Cloudinary? _cloudinary;
        private readonly bool _useCloudinary;
        private readonly string? _localVideoRoot;

        public CloudinaryVideoService(IConfiguration config, IHostEnvironment environment)
        {
            var cloudName = config["Cloudinary:CloudName"];
            var apiKey = config["Cloudinary:ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"];

            if (!string.IsNullOrWhiteSpace(cloudName)
                && !string.IsNullOrWhiteSpace(apiKey)
                && !string.IsNullOrWhiteSpace(apiSecret))
            {
                Account account = new Account(cloudName, apiKey, apiSecret);
                _cloudinary = new Cloudinary(account);
                _useCloudinary = true;
            }
            else
            {
                _useCloudinary = false;
                var webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");
                if (!Directory.Exists(webRoot))
                {
                    webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
                }

                _localVideoRoot = Path.Combine(webRoot, "videos");
                Directory.CreateDirectory(_localVideoRoot);
            }
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
            if (!_useCloudinary)
            {
                var extension = Path.GetExtension(fileName);
                var safeName = $"{Guid.NewGuid():N}{extension}";
                var fullPath = Path.Combine(_localVideoRoot!, safeName);

                if (fileStream.CanSeek)
                {
                    fileStream.Position = 0;
                }

                await using var outputStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await fileStream.CopyToAsync(outputStream);
                return $"/videos/{safeName}";
            }

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
            if (!_useCloudinary)
            {
                try
                {
                    var fileName = Path.GetFileName(url.TrimStart('/'));
                    var fullPath = Path.Combine(_localVideoRoot!, fileName);
                    if (!File.Exists(fullPath)) return false;
                    File.Delete(fullPath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
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

                if (!_useCloudinary)
                {
                    return videoUrl;
                }

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
