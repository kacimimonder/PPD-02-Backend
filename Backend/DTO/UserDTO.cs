using Application.DTOs.User;

namespace API.DTO
{
    public class UserDTO
    {
        public UserCreateDTO userCreateDTO { get; set; } = default!;
        public IFormFile? image { get; set; }
    }
}
