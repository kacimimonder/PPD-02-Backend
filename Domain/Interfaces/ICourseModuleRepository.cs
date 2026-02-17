using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICourseModuleRepository:IBaseRepository<CourseModule>
    {
        Task<bool> IsCourseModuleCreatedByInstructor(int instructorId, int courseModuleId);
    }
}
