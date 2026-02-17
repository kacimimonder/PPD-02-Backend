using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class EnrollmentProgress
    {
        // Both (EnrollmentId && ModuleContentId) must be unique
        // EnrollmentId should be indexed
        public int Id { get; set; } = default!;
        public int EnrollmentId { get; set; } = default!;
        public Enrollment? Enrollment { get; set; }
        public int ModuleContentId { get; set; } = default!;
        public ModuleContent? ModuleContent { get; set; }


    }
}
