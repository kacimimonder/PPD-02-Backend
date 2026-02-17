using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ModuleContent
{
    public class ModuleContentCreateDTO
    {
        public string Name { get; set; } = default!;
        public string? Content { get; set; }
        public int CourseModuleID { get; set; } = default!;
    }

}
