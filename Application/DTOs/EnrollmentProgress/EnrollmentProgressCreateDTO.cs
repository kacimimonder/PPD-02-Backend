using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.EnrollmentProgress
{
    public class EnrollmentProgressCreateDTO
    {
        public int EnrollmentId { get; set; } = default!;
        public int ModuleContentId { get; set; } = default!;
    }
}
