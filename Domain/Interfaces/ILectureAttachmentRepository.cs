using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ILectureAttachmentRepository : IBaseRepository<LectureAttachment>
    {
        Task<List<LectureAttachment>> GetByModuleContentIdAsync(int moduleContentId);
        Task<List<LectureAttachment>> GetByIdsAsync(IEnumerable<int> ids);
        Task AddRangeAsync(IEnumerable<LectureAttachment> attachments);
        Task DeleteRangeAsync(IEnumerable<LectureAttachment> attachments);
    }
}
