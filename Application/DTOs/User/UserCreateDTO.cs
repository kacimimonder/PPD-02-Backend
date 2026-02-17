using Domain.Enums;

namespace Application.DTOs.User
{
    public class UserCreateDTO
    {
        public UserTypeEnum UserType { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string? LastName { get; set; }
        // Email should be unique 
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;

    }
}
