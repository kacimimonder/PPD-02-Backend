using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ModuleContent
{
    public class ModuleContentUpdateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Content { get; set; }
        public bool DeleteVideo { get; set; } = false;
    }
}
