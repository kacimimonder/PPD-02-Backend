using Application.DTOs.Course;
using Application.DTOs.CourseModule;
using Application.DTOs.Other;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Utilities;
using Domain.Models;

namespace Application.Services
{
    public class CourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IImageStorageService _imageStorage;
        private readonly IMapper _mapper;
        private readonly IVideoService _videoService;
        private readonly IEnrollmentRepository _enrollmentRepository;

        public CourseService(ICourseRepository courseRepository,
            IImageStorageService imageStorage, IMapper mapper, 
            IVideoService videoService, IEnrollmentRepository enrollmentRepository)
        {
            _courseRepository = courseRepository;
            _imageStorage = imageStorage;
            _mapper = mapper;
            _videoService = videoService;
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<List<Course>> GetNewCoursesAsync(int amount = 4)
        {
            return await _courseRepository.GetNewCoursesAsync(amount);
        }
        public async Task<List<Course>> GetPopularCoursesAsync(int amount = 4)
        {
            return await _courseRepository.GetPopularCoursesAsync(amount);
        }
        public async Task<List<Course>> GetDiscoverCoursesAsync(int amount = 4)
        {
            return await _courseRepository.GetDiscoverCoursesAsync(amount);
        }
        public async Task<List<Course>> GetSearchedCoursesAsync(string searchTerm, int amount)
        {
            return await _courseRepository.GetSearchedCoursesAsync(searchTerm, amount);
        }
        public async Task<int> CreateCourseAsync(CourseCreateDTO courseCreateDTO, Stream imageStream)
        {
            var imageUrl = await _imageStorage.SaveImageAsync(imageStream);
            Course course = _mapper.Map<Course>(courseCreateDTO);
            course.ImageUrl = imageUrl;
            await _courseRepository.AddAsync(course);
            return course.Id;
        }
        public async Task<List<Course>> GetCoursesByFilterAsync(FilterCoursesDTO filterCoursesDTO)
        {
            var filterCoursesModel = _mapper.Map<FilterCoursesModel>(filterCoursesDTO);
            return await _courseRepository.GetCoursesByFilterAsync(filterCoursesModel);
        }
        public async Task<Course>? GetByIdAsync(int id)
        {
            return await _courseRepository.GetByIdAsync(id);
        }
        public async Task<List<CourseModuleReadDTO>> GetCourseModulesContentsAsync(int courseId,int userId,string role)
        {
            if(role == "Student")
            {
                bool result = await _enrollmentRepository
                    .IsStudentEnrolledInCourse(userId, courseId);
                if (!result) throw new ForbiddenException($"As a student you must be 1st enrolled in the course with Id = {courseId} to view its content");
            }
            else
            {
                bool result = await _courseRepository
                    .IsCourseCreatedByInstructor(userId, courseId);
                if (!result) throw new ForbiddenException($"As an instructor you don't have the course with Id = {courseId}");
            }
            List<CourseModule> courseModules = await _courseRepository.GetCourseModulesContentsAsync(courseId);
            foreach (CourseModule courseModule in courseModules)
            {
                foreach (ModuleContent content in courseModule.ModuleContents)
                {
                    if (!string.IsNullOrEmpty(content.VideoUrl))
                    {
                        content.VideoUrl = _videoService.GetStreamingUrl(content.VideoUrl, signUrl: true);
                    }
                }
            }
            List<CourseModuleReadDTO> courseModuleReadDTOs = _mapper.Map<List<CourseModuleReadDTO>>(courseModules);
            return courseModuleReadDTOs;
        }
        public async Task UpdateCourseAsync(int instructorId, int courseId,
            CourseCreateDTO courseCreateDTO, Stream? imageStream)
        {
            Course? oldCourse = await _courseRepository.GetByIdAsync(courseId);
            if (oldCourse == null) {
                throw new NotFoundException("Course not found");
            }
            bool isCourseCreatedByInstructor = await _courseRepository.IsCourseCreatedByInstructor(instructorId, courseId);
            if (!isCourseCreatedByInstructor) throw new ForbiddenException("You can't update this course because it's not yours");
            
            string imageUrl = oldCourse.ImageUrl;
            if (imageStream != null) {
                await _imageStorage.DeleteImageAsync(oldCourse.ImageUrl);
                imageUrl = await _imageStorage.SaveImageAsync(imageStream);
            }
            Course updatedCourse = _mapper.Map<Course>(courseCreateDTO);
            updatedCourse.Id = courseId;
            updatedCourse.ImageUrl = imageUrl;
            await _courseRepository.UpdateAsync(updatedCourse);
        }
        public async Task<List<Course>> GetInstructorCoursesAsync(int instructorId)
        {
            return await _courseRepository.GetInstructorCoursesAsync(instructorId);
        }
        public async Task<int> GetEnrollmentsCountByCourseId(int courseId)
        {
            return await _courseRepository.GetEnrollmentsCountByCourseId(courseId);
        }

    }
}
