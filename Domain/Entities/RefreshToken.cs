using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class RefreshToken
    {
        public int TokenId { get; set; }
        public string Token { get; set; } = default!;
        public DateTime ExpiresOn { get; set; }
        public bool IsExpired => ExpiresOn <= DateTime.UtcNow;
        public DateTime CreatedOn { get; set; }
        public DateTime? RevokedOn { get; set; }
        public bool IsActive => RevokedOn == null && !IsExpired;
        public User? User { get; set; }
        public int UserId { get; set; } = default!;
    }
}
