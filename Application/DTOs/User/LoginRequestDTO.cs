using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.User
{
    public class LoginRequestDTO
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
