using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.CourseModule;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class CourseModuleService
    {
        private readonly ICourseModuleRepository _courseModuleRepository;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;

        public CourseModuleService(ICourseModuleRepository courseModuleRepository, IMapper mapper, ICourseRepository courseRepository)
        {
            _courseModuleRepository = courseModuleRepository;
            _mapper = mapper;
            _courseRepository = courseRepository;
        }

        public async Task<int> CreateCourseModuleAsync(CourseModuleCreateDTO courseModuleCreateDTO, int instructorId)
        {
            List<Course> createdCoursesByInstructor = await _courseRepository.GetInstructorCoursesAsync(instructorId);
            if(!createdCoursesByInstructor.Exists(course => course.Id == courseModuleCreateDTO.CourseId))
            {
                throw new ForbiddenException($"Instructor with Id {instructorId} dosen't have the right to add a module to this course");
            }
            CourseModule courseModule = _mapper.Map<CourseModule>(courseModuleCreateDTO);
            await _courseModuleRepository.AddAsync(courseModule);
            return courseModule.Id;
        }

        public async Task UpdateCourseModuleAsync(int id, CourseModuleCreateDTO courseModuleCreateDTO, int instructorId)
        {
            List<Course> createdCoursesByInstructor = await _courseRepository.GetInstructorCoursesAsync(instructorId);
            List<CourseModule> courseModules = createdCoursesByInstructor.SelectMany(course => course.CourseModules).ToList();
            if (!courseModules.Exists(courseModule => courseModule.Id == id))
            {
                throw new ForbiddenException($"Instructor with Id {instructorId} dosen't have the right to update the course module with Id {id}");
            }
            CourseModule courseModule = await _courseModuleRepository.GetByIdAsync(id);
            if (courseModule == null)
            {
                throw new Exception($"CourseModule with ID {id} not found.");
            }

            // Map the updated fields from the DTO to the existing entity

            courseModule.Description = courseModuleCreateDTO.Description;
            courseModule.Name = courseModuleCreateDTO.Name;
            // Save the changes
            await _courseModuleRepository.UpdateAsync(courseModule);
        }

        public async Task DeleteCourseModuleAsync(int id, int instructorId)
        {
            List<Course> createdCoursesByInstructor = await _courseRepository.GetInstructorCoursesAsync(instructorId);
            List<CourseModule> courseModules = createdCoursesByInstructor.SelectMany(course => course.CourseModules).ToList();
            if (!courseModules.Exists(courseModule => courseModule.Id == id))
            {
                throw new ForbiddenException($"Instructor with Id {instructorId} dosen't have the right to delete the course module with Id {id}");
            }
            //var courseModule = await _courseModuleRepository.GetByIdAsync(id);
            //if (courseModule == null)
            //{
            //    throw new NotFoundException($"CourseModule with ID {id} not found.");
            //}

            await _courseModuleRepository.DeleteAsync(id);
        }


    }
}
