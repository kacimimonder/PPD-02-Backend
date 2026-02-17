using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Subject
    {
        public int SubjectId { get; set; }

        [Required]
        public string Name { get; set; }

        // Navigation property for the related courses
        public ICollection<Course>? Courses { get; set; }
    }
}
