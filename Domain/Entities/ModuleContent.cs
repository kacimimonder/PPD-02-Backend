using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ModuleContent
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Content { get; set; }
        public string? VideoUrl { get; set; }
        public int CourseModuleID { get; set; } = default!;
        public CourseModule? courseModule { get; set; }
    }
}
