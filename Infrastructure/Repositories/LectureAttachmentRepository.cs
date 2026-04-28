using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class LectureAttachmentRepository : ILectureAttachmentRepository
    {
        private readonly MiniCourseraContext _miniCourseraContext;

        public LectureAttachmentRepository(MiniCourseraContext miniCourseraContext)
        {
            _miniCourseraContext = miniCourseraContext;
        }

        public async Task AddAsync(LectureAttachment entity)
        {
            await _miniCourseraContext.LectureAttachments.AddAsync(entity);
            await _miniCourseraContext.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<LectureAttachment> attachments)
        {
            await _miniCourseraContext.LectureAttachments.AddRangeAsync(attachments);
            await _miniCourseraContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var attachment = await _miniCourseraContext.LectureAttachments.FindAsync(id);
            if (attachment == null)
            {
                return;
            }

            _miniCourseraContext.LectureAttachments.Remove(attachment);
            await _miniCourseraContext.SaveChangesAsync();
        }

        public async Task DeleteRangeAsync(IEnumerable<LectureAttachment> attachments)
        {
            _miniCourseraContext.LectureAttachments.RemoveRange(attachments);
            await _miniCourseraContext.SaveChangesAsync();
        }

        public Task<List<LectureAttachment>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<LectureAttachment?> GetByIdAsync(int id)
        {
            return await _miniCourseraContext.LectureAttachments.FirstOrDefaultAsync(attachment => attachment.Id == id);
        }

        public async Task<List<LectureAttachment>> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await _miniCourseraContext.LectureAttachments
                .Where(attachment => ids.Contains(attachment.Id))
                .ToListAsync();
        }

        public async Task<List<LectureAttachment>> GetByModuleContentIdAsync(int moduleContentId)
        {
            return await _miniCourseraContext.LectureAttachments
                .Where(attachment => attachment.ModuleContentId == moduleContentId)
                .ToListAsync();
        }

        public Task UpdateAsync(LectureAttachment entity)
        {
            throw new NotImplementedException();
        }
    }
}
