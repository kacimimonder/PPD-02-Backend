using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class CourseModuleRepository:ICourseModuleRepository
    {
        MiniCourseraContext _miniCourseraContext { get; set; }
        public CourseModuleRepository(MiniCourseraContext miniCourseraContext) { 
            _miniCourseraContext = miniCourseraContext;
        }

        public async Task AddAsync(CourseModule entity)
        {
            _miniCourseraContext.CourseModules.Add(entity);
            await _miniCourseraContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var courseModule = await _miniCourseraContext.CourseModules.FindAsync(id);
            if (courseModule != null)
            {
                _miniCourseraContext.CourseModules.Remove(courseModule);
                await _miniCourseraContext.SaveChangesAsync();
            }
        }
        public async Task<bool> IsCourseModuleCreatedByInstructor(int instructorId, int courseModuleId)
        {
            return await _miniCourseraContext.CourseModules
                .Include(courseModule => courseModule.Course)
                .AnyAsync(courseModule => courseModule.Id == courseModuleId
                && courseModule.Course.InstructorID == instructorId);
        }
        public Task<List<CourseModule>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<CourseModule?> GetByIdAsync(int id)
        {
            return await _miniCourseraContext.CourseModules.FindAsync(id);
        }

        public async Task UpdateAsync(CourseModule entity)
        {
            var existingModule = await _miniCourseraContext.CourseModules.FindAsync(entity.Id);

            if (existingModule == null)
            {
                throw new Exception($"CourseModule with ID {entity.Id} not found.");
            }

            // Update the properties you want to allow changing
            existingModule.Name = entity.Name;
            existingModule.Description = entity.Description;

            await _miniCourseraContext.SaveChangesAsync();
        }


    }
}
