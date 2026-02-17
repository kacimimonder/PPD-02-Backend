using API.DTO;
using API.Utilities;
using Application.DTOs.Course;
using Application.DTOs.RefreshToken;
using Application.DTOs.User;
using Application.Exceptions;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        IMapper _mapper;
        UserService _userService;
        //JwtUtil _jwtUtil;
        public UserController(IMapper mapper, UserService userService)
        {
            _mapper = mapper;
            _userService = userService;
            //_jwtUtil = jwtUtil;
        }



        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUser([FromForm]UserDTO userDTO)
        {
            UserCreateDTO userCreateDTO = userDTO.userCreateDTO;
            IFormFile? image = userDTO.image;
            if (userCreateDTO == null)
            {
                return BadRequest("User data is null");
            }

            if (string.IsNullOrWhiteSpace(userCreateDTO.FirstName))
            {
                return BadRequest("First name cannot be empty");
            }

            try
            {
                Stream? imageStream = null;

                if (image != null)
                {
                    imageStream = image.OpenReadStream();
                }

                await _userService.CreateUserAsync(userCreateDTO, imageStream);

                return Ok("User created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while creating the user: {ex.Message}");
            }
        }



        [HttpPost("login")]
        [ProducesResponseType(typeof(UserReadDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserReadDTO>> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            try
            {
                var userDTO = await _userService.GetByEmailAndPasswordAsync(loginRequestDTO.Email, loginRequestDTO.Password);
                if (userDTO == null)
                {
                    return NotFound("No available user");
                }
                HttpContext.Response.Cookies.Append("refreshToken", userDTO.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // only over HTTPS
                    SameSite = SameSiteMode.None, // 👈 Required for cross-origin cookies
                    Expires = userDTO.RefreshTokenExpiration
                });
                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }



        [HttpPost("refreshToken")]
        [ProducesResponseType(typeof(UserReadDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                string? refreshToken = Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken)) return BadRequest("Invalid request");
                UserReadDTO? userReadDTO = await _userService.RefreshTokens(refreshToken);
                HttpContext.Response.Cookies.Append("refreshToken", userReadDTO.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // only over HTTPS
                    SameSite = SameSiteMode.None, // 👈 Required for cross-origin cookies
                    Expires = userReadDTO.RefreshTokenExpiration
                });
                return Ok(userReadDTO);
            }
            catch (BadRequestException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error adding course module: {ex.Message}");
            }
        }


    }
}
