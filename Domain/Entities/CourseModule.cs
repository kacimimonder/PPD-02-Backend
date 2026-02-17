using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CourseModule
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public ICollection<ModuleContent>? ModuleContents { get; set; }
    }
}
