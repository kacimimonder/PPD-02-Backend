using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public UserTypeEnum UserType { get; set; } = default!; 
        public string FirstName { get; set; } = default!;
        public string? LastName { get; set; }
        // Email should be unique 
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string? PhotoUrl { get; set; }
        public string? AiAmbitions { get; set; }
        public string? AiInterests { get; set; }
        public DateTime? AiProfileUpdatedAtUtc { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public string FullName => $"{FirstName} {LastName}".Trim();
        public ICollection<RefreshToken>? RefreshTokens { get; set; }

    }
}
