using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ModuleContent
{
    public class ModuleContentReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Content { get; set; }
        public string? VideoUrl { get; set; }
    }
}
