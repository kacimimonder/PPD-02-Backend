using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ModuleContentRepository : IModuleContentRepository
    {
        private MiniCourseraContext _miniCourseraContext;
        public ModuleContentRepository(MiniCourseraContext miniCourseraContext)
        {
            _miniCourseraContext = miniCourseraContext;
        }

        public async Task AddAsync(ModuleContent entity)
        {
            _miniCourseraContext.ModuleContents.Add(entity);
            await _miniCourseraContext.SaveChangesAsync();
        }
        public async Task UpdateAsync(ModuleContent entity)
        {
            var existingEntity = await _miniCourseraContext.ModuleContents.FindAsync(entity.Id);
            if (existingEntity == null)
            {
                throw new InvalidOperationException($"ModuleContent with ID {entity.Id} not found.");
            }

            // Update fields
            existingEntity.Name = entity.Name;
            existingEntity.Content = entity.Content;
            existingEntity.VideoUrl = entity.VideoUrl;

            await _miniCourseraContext.SaveChangesAsync();
        }
        public async Task<ModuleContent?> GetByIdAsync(int id)
        {
            try
            {
                return await _miniCourseraContext.ModuleContents.FindAsync(id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task DeleteAsync(int id)
        {
            var moduleContent = await _miniCourseraContext.ModuleContents.FindAsync(id);
            if (moduleContent == null)
            {
                throw new KeyNotFoundException($"Module content with ID {id} not found.");
            }

            _miniCourseraContext.ModuleContents.Remove(moduleContent);
            await _miniCourseraContext.SaveChangesAsync();
        }
        public async Task<bool> IsModuleContentCreatedByInstructor(int instructorId, 
            int moduleContentId)
        {
            return await _miniCourseraContext.ModuleContents
                .Include(moduleContent => moduleContent.courseModule)
                    .ThenInclude(courseModule => courseModule.Course)
                .AnyAsync(moduleContent => moduleContent.Id == moduleContentId
                && moduleContent.courseModule.Course.InstructorID == instructorId);
        }


        public Task<List<ModuleContent>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

    }

}
