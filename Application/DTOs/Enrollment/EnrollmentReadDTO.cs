using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.EnrollmentProgress;

namespace Application.DTOs.Enrollment
{
    public class EnrollmentReadDTO
    {
        public int Id { get; set; }
        public int CourseID { get; set; }
        public bool IsCompleted { get; set; } = false;
        public string? CourseTitle { get; set; }
        public List<EnrollmentProgressCreateDTO>? EnrollmentProgress { get; set; }
    }
}
