using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface ICourseRepository : IBaseRepository<Course>
    {
        Task<List<Course>> GetNewCoursesAsync(int amount = 4);
        Task<List<Course>> GetPopularCoursesAsync(int amount = 4);
        Task<List<Course>> GetDiscoverCoursesAsync(int amount = 4);
        Task<List<Course>> GetSearchedCoursesAsync(string searchTerm, int amount = 4);
        Task<List<Course>> GetCoursesByFilterAsync(FilterCoursesModel filterCoursesModel);
        Task<List<CourseModule>> GetCourseModulesContentsAsync(int courseID);
        Task<List<Course>> GetInstructorCoursesAsync(int instructorId);
        Task<int> GetEnrollmentsCountByCourseId(int courseId);
        Task<bool> IsCourseCreatedByInstructor(int instructorId, int courseId);
    }
}
