using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IModuleContentRepository: IBaseRepository<ModuleContent>
    {
        Task<bool> IsModuleContentCreatedByInstructor(int instructorId,
            int moduleContentId);
    }

}
