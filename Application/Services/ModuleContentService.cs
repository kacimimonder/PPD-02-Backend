using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.ModuleContent;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Utilities;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ModuleContentService
    {
        private IModuleContentRepository _moduleContentRepository;
        private readonly ILectureAttachmentRepository _lectureAttachmentRepository;
        private ICourseModuleRepository _courseModuleRepository;
        private IMapper _mapper;
        private IVideoService _videoService;
        private readonly ILectureAttachmentStorageService _lectureAttachmentStorageService;
        private readonly ILogger<ModuleContentService> _logger;
        public ModuleContentService(
            IModuleContentRepository moduleContentRepository,
            ILectureAttachmentRepository lectureAttachmentRepository,
            IMapper mapper,
            IVideoService videoService,
            ICourseModuleRepository courseModuleRepository,
            ILectureAttachmentStorageService lectureAttachmentStorageService,
            ILogger<ModuleContentService> logger)
        {
            _moduleContentRepository = moduleContentRepository;
            _lectureAttachmentRepository = lectureAttachmentRepository;
            _mapper = mapper;
            _videoService = videoService;
            _courseModuleRepository = courseModuleRepository;
            _lectureAttachmentStorageService = lectureAttachmentStorageService;
            _logger = logger;
        }

        public async Task<int> AddModuleContentAsync(int instructorId, 
            ModuleContentCreateDTO moduleContentCreateDTO, Stream videoStream,
            string fileName)
        {
            bool result = await _courseModuleRepository
                .IsCourseModuleCreatedByInstructor(instructorId, moduleContentCreateDTO.CourseModuleID);
            if (!result) throw new BadRequestException($"Either you don't have the right to create this module content or the course module with id = {moduleContentCreateDTO.CourseModuleID} dosen't exist");
            ModuleContent moduleContent = _mapper.Map<ModuleContent>(moduleContentCreateDTO);
            if (moduleContent == null)
            {
                throw new ArgumentNullException(nameof(moduleContent), "Module content cannot be null.");
            }
            if (videoStream != null)
            {
                string videoUrl = await _videoService.UploadVideoAsync(videoStream, fileName);
                moduleContent.VideoUrl = videoUrl;
            }
            await _moduleContentRepository.AddAsync(moduleContent);

            return moduleContent.Id;
        }

        public async Task<int> AddModuleContentAttachmentsAsync(
            int instructorId,
            int moduleContentId,
            IEnumerable<ModuleContentAttachmentUploadDTO> attachments)
        {
            if (attachments == null)
            {
                return 0;
            }

            var uploadList = attachments.ToList();
            if (uploadList.Count == 0)
            {
                return 0;
            }

            var lecture = await _moduleContentRepository.GetByIdWithAttachmentsAsync(moduleContentId)
                ?? throw new BadRequestException($"Module content with id = {moduleContentId} doesn't exist");

            if (!await _courseModuleRepository.IsCourseModuleCreatedByInstructor(instructorId, lecture.CourseModuleID))
            {
                throw new BadRequestException("Either you don't have the right to create this module content or the course module does not exist");
            }

            var savedPaths = new List<string>();
            var attachmentEntities = new List<LectureAttachment>();

            try
            {
                foreach (var item in uploadList)
                {
                    ValidateAttachment(item);
                    var fileUrl = await _lectureAttachmentStorageService.SaveAttachmentAsync(item.FileBytes, item.FileName, item.ContentType);
                    savedPaths.Add(fileUrl);

                    attachmentEntities.Add(new LectureAttachment
                    {
                        ModuleContentId = moduleContentId,
                        FileName = item.FileName,
                        FileUrl = fileUrl,
                        ContentType = item.ContentType,
                        AttachmentType = item.AttachmentType,
                        CreatedAtUtc = DateTime.UtcNow
                    });
                }

                await _lectureAttachmentRepository.AddRangeAsync(attachmentEntities);
                return attachmentEntities.Count;
            }
            catch
            {
                foreach (var path in savedPaths)
                {
                    await _lectureAttachmentStorageService.DeleteAttachmentAsync(path);
                }
                throw;
            }
        }

        public async Task UpdateModuleContentAsync(int instructorId,
            ModuleContentUpdateDTO dto, Stream? videoStream, string? fileName)
        {
            bool result = await _moduleContentRepository.IsModuleContentCreatedByInstructor(
                instructorId, dto.Id);
            if (!result) throw new BadRequestException("Either the instructor dosen't have the right to update this module content or the module content itself dosen't exist");
            var moduleContent = _mapper.Map<ModuleContent>(dto);
            var original = await _moduleContentRepository.GetByIdWithAttachmentsAsync(moduleContent.Id);
            if (original == null) return;
            moduleContent.VideoUrl = original.VideoUrl;
            if (dto.DeleteVideo)
            {
                await _videoService.DeleteVideoAsync(original.VideoUrl);
                moduleContent.VideoUrl = null;
                if (videoStream != null && !string.IsNullOrEmpty(fileName))
                {
                    string newUrl = await _videoService.UploadVideoAsync(videoStream, fileName);
                    moduleContent.VideoUrl = newUrl;
                }
            }

            await _moduleContentRepository.UpdateAsync(moduleContent);
        }

        public async Task SyncModuleContentAttachmentsAsync(
            int instructorId,
            int moduleContentId,
            IEnumerable<int>? deleteAttachmentIds,
            IEnumerable<ModuleContentAttachmentUploadDTO>? newAttachments)
        {
            var moduleContent = await _moduleContentRepository.GetByIdWithAttachmentsAsync(moduleContentId)
                ?? throw new BadRequestException($"Module content with id = {moduleContentId} doesn't exist");

            if (!await _courseModuleRepository.IsCourseModuleCreatedByInstructor(instructorId, moduleContent.CourseModuleID))
            {
                throw new BadRequestException("Either you don't have the right to update this module content or the course module does not exist");
            }

            var deleteIds = deleteAttachmentIds?.Distinct().ToList() ?? new List<int>();
            if (deleteIds.Count > 0)
            {
                var attachmentsToDelete = moduleContent.LectureAttachments
                    .Where(attachment => deleteIds.Contains(attachment.Id))
                    .ToList();

                foreach (var attachment in attachmentsToDelete)
                {
                    await _lectureAttachmentStorageService.DeleteAttachmentAsync(attachment.FileUrl);
                }

                if (attachmentsToDelete.Count > 0)
                {
                    await _lectureAttachmentRepository.DeleteRangeAsync(attachmentsToDelete);
                }
            }

            if (newAttachments != null)
            {
                await AddModuleContentAttachmentsAsync(instructorId, moduleContentId, newAttachments);
            }
        }

        public async Task DeleteModuleContentAsync(int instructorId, int id)
        {
            var moduleContent = await _moduleContentRepository.GetByIdWithAttachmentsAsync(id);
            if (moduleContent == null)
            {
                throw new NotFoundException($"Module content with ID {id} not found.");
            }
            bool result = await _moduleContentRepository.IsModuleContentCreatedByInstructor(
    instructorId, id);
            if (!result) throw new ForbiddenException("The instructor dosen't have the right to delete this module content");


            // Optional: delete the associated video from cloud storage if it exists
            if (!string.IsNullOrEmpty(moduleContent.VideoUrl))
            {
                await _videoService.DeleteVideoAsync(moduleContent.VideoUrl);
            }

            if (moduleContent.LectureAttachments?.Any() == true)
            {
                foreach (var attachment in moduleContent.LectureAttachments)
                {
                    await _lectureAttachmentStorageService.DeleteAttachmentAsync(attachment.FileUrl);
                }

                await _lectureAttachmentRepository.DeleteRangeAsync(moduleContent.LectureAttachments);
            }

            await _moduleContentRepository.DeleteAsync(id);
        }

        private static void ValidateAttachment(ModuleContentAttachmentUploadDTO attachment)
        {
            if (attachment.FileBytes == null || attachment.FileBytes.Length == 0)
            {
                throw new BadRequestException("Attachment file cannot be empty.");
            }

            var ext = Path.GetExtension(attachment.FileName).ToLowerInvariant();
            var contentType = attachment.ContentType ?? string.Empty;
            var isPdf = attachment.AttachmentType == "Pdf" && ext == ".pdf" && contentType == "application/pdf";
            var isImage = attachment.AttachmentType == "Image" && (ext is ".jpg" or ".jpeg" or ".png" or ".webp") && contentType.StartsWith("image/");

            if (!isPdf && !isImage)
            {
                throw new BadRequestException("Only PDF and image attachments are allowed.");
            }
        }


    }
}
