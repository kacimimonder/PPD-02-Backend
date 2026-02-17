using System;
using System.IO;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Interfaces.Utilities;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Utilities
{
    public class LocalImageStorageService : IImageStorageService
    {
        private readonly Cloudinary _cloudinary;

        public LocalImageStorageService(IConfiguration config)
        {
            var cloudName = config["Cloudinary:CloudName"];
            var apiKey = config["Cloudinary:ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"];

            Account account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> SaveImageAsync(Stream imageStream)
        {
            try
            {
                if (imageStream == null || imageStream.Length == 0)
                    return null;

                var uniqueFileName = $"{Guid.NewGuid()}.jpg"; // Optional: detect format
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(uniqueFileName, imageStream),
                    Folder = "course_images"
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                return uploadResult.SecureUrl.ToString();
            }
            catch
            {
                return null;
            }       
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl)) return false;

                var publicId = ExtractPublicIdFromUrl(imageUrl);

                var deletionParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image
                };

                var result = await _cloudinary.DestroyAsync(deletionParams);
                return result.Result == "ok";
            }
            catch { 
                return false;
            }
        }

        private string ExtractPublicIdFromUrl(string url)
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/');
            var filename = segments.Last();         // abc123_xyz.jpg
            var folder = segments[^2];              // course_images
            var publicId = Path.Combine(folder, Path.GetFileNameWithoutExtension(filename)).Replace('\\', '/');
            return publicId;
        }

    }
}
