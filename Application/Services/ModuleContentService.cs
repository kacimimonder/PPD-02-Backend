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

namespace Application.Services
{
    public class ModuleContentService
    {
        private IModuleContentRepository _moduleContentRepository;
        private ICourseModuleRepository _courseModuleRepository;
        private IMapper _mapper;
        private IVideoService _videoService;
        public ModuleContentService(
            IModuleContentRepository moduleContentRepository, IMapper mapper
            ,IVideoService videoService, ICourseModuleRepository courseModuleRepository)
        {
            _moduleContentRepository = moduleContentRepository;
            _mapper = mapper;
            _videoService = videoService;
            _courseModuleRepository = courseModuleRepository;
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

        public async Task UpdateModuleContentAsync(int instructorId,
            ModuleContentUpdateDTO dto, Stream? videoStream, string? fileName)
        {
            bool result = await _moduleContentRepository.IsModuleContentCreatedByInstructor(
                instructorId, dto.Id);
            if (!result) throw new BadRequestException("Either the instructor dosen't have the right to update this module content or the module content itself dosen't exist");
            var moduleContent = _mapper.Map<ModuleContent>(dto);
            var original = await _moduleContentRepository.GetByIdAsync(moduleContent.Id);
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

        public async Task DeleteModuleContentAsync(int instructorId, int id)
        {
            var moduleContent = await _moduleContentRepository.GetByIdAsync(id);
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

            await _moduleContentRepository.DeleteAsync(id);
        }


    }
}
