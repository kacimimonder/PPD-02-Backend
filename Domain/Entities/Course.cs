using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string ImageUrl { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public int InstructorID { get; set; } = default!;
        public User Instructor { get; set; } = default!;
        public ICollection<Enrollment> Enrollments { get; set; }
        public int EnrollmentsCount { get; set; }
        public int? SubjectID { get; set; }
        public Subject? Subject { get; set; }
        public int LanguageID { get; set; } = 1;
        public Language Language { get; set; }
        public ICollection<CourseModule> CourseModules { get; set; }
        public CourseLevelEnum Level { get; set; } 

        public Course()
        {
            CreatedAt = DateTime.UtcNow;
        }

    }
}
