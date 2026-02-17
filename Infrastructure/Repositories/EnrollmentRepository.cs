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
    public class EnrollmentRepository : IEnrollmentRepository
    {
        MiniCourseraContext _miniCourseraContext { get; set; }
        public EnrollmentRepository(MiniCourseraContext miniCourseraContext)
        {
            _miniCourseraContext = miniCourseraContext;
        }

        public async Task AddAsync(Enrollment entity)
        {
            await _miniCourseraContext.AddAsync(entity);
            _miniCourseraContext.SaveChanges();
        }

        public async Task<List<Enrollment>> GetEnrolledCoursesByStudentId(int studentId)
        {
            try
            {
                var enrolledCourses = await _miniCourseraContext.Enrollments
                    .Where(enrollment => enrollment.StudentId == studentId)
                    .Include(Enrollment => Enrollment.Course)
                    .ToListAsync();
                return enrolledCourses;
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                throw new Exception("An error occurred while retrieving enrolled courses", ex);

            }

        }

        public async Task<Enrollment?> GetEnrollmentByCourseIdAndStudentId(int courseId, int studentId)
        {
            return await _miniCourseraContext.Enrollments
                .Include(enrollment => enrollment.enrollmentProgresses)
                .FirstOrDefaultAsync(
                enrollment => enrollment.CourseId == courseId && 
                enrollment.StudentId == studentId);
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Enrollment>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Enrollment?> GetByIdAsync(int id)
        {
            return await _miniCourseraContext.Enrollments
                .FirstOrDefaultAsync(enroll => enroll.Id == id);
        }

        public async Task<Enrollment?> GetEnrollmentWithProgressAndCourse(int EnrollmentId)
        {
            return await _miniCourseraContext.Enrollments
                .Include(enrollment => enrollment.Course)
                    .ThenInclude(course => course.CourseModules)
                        .ThenInclude(courseModule => courseModule.ModuleContents)
                .Include(enrollment => enrollment.enrollmentProgresses)
                .FirstOrDefaultAsync(enrollment => enrollment.Id == EnrollmentId);
        }

        public async Task<bool> IsStudentEnrolledInCourse(int studentId, int courseId)
        {
            return await _miniCourseraContext.Enrollments
                .AnyAsync(enroll => enroll.CourseId == courseId
                && enroll.StudentId == studentId);
        }

        public Task UpdateAsync(Enrollment entity)
        {
            throw new NotImplementedException();
        }
    }
}
