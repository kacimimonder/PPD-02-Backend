using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.ModuleContent;

namespace Application.DTOs.CourseModule
{
    public class CourseModuleReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public List<ModuleContentReadDTO>? ModuleContents { get; set; }
    }

}
