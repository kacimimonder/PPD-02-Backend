using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.CourseModule;

namespace Application.DTOs.Course
{
    public class CourseReadFullDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string ImageUrl { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public string InstructorName { get; set; } = default!;
        public string InstructorImageUrl { get; set; } = default!;
        public string Category { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int EnrollmentsCount { get; set; } = default!;
        public string Language { get; set; } = default!;
        public string Level { get; set; } = default!;
        public List<CourseModuleReadDTO>? Modules { get; set; }

    }
}
