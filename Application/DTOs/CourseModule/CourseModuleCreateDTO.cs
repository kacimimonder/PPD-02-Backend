using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.CourseModule
{
    public class CourseModuleCreateDTO
    {
        public int? CourseId { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
    }
}
