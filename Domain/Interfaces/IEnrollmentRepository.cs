using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IEnrollmentRepository:IBaseRepository<Enrollment>
    {
        Task <List<Enrollment>> GetEnrolledCoursesByStudentId(int studentId);
        Task <Enrollment?> GetEnrollmentByCourseIdAndStudentId(int courseId, int studentId);
        Task<Enrollment?> GetEnrollmentWithProgressAndCourse(int EnrollmentId);
        Task <bool> IsStudentEnrolledInCourse(int studentId,int courseId);
    }
}
