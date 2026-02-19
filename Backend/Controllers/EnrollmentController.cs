using Application.DTOs.Course;
using Application.DTOs.Enrollment;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        public EnrollmentService _enrollmentService { get; set; }
        public EnrollmentController(EnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }



        [Authorize (Roles = "Student")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddEnrollmentAsync([FromBody] EnrollmentCreateDTO enrollmentCreateDTO)
        {
            var nameId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(nameId))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, "Token missing or invalid.");
            }
            int studentId = int.Parse(nameId);
            if (enrollmentCreateDTO == null || enrollmentCreateDTO.CourseId <= 0)
            {
                return BadRequest("Enrollment data is null or invalid courseId.");
            }
            try
            {
                await _enrollmentService.AddEnrollmentAsync(enrollmentCreateDTO.CourseId, studentId);
                Console.WriteLine($"[Enrollment] Added: StudentId={studentId}, CourseId={enrollmentCreateDTO.CourseId}");
                return Ok("Enrollment added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error adding enrollment: {ex.Message}");
            }
        }




        [Authorize(Roles = "Student")]
        [HttpGet("studentEnrolledCourses")]
        [ProducesResponseType(typeof(List<EnrollmentReadDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<EnrollmentReadDTO>>> GetEnrolledCoursesByStudentId()
        {
            int studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                var enrollmentReadDTOs = await _enrollmentService.GetEnrolledCoursesByStudentId(studentId);
                if (!enrollmentReadDTOs.Any())
                {
                    return NotFound("No enrolled courses available");
                }

                return Ok(enrollmentReadDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }




        [Authorize(Roles = "Student")]
        [HttpGet("by-course-and-student")]
        [ProducesResponseType(typeof(List<EnrollmentReadDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EnrollmentReadDTO>> GetEnrollmentByCourseIdAndStudentId(
     int courseId)
        {
            int studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (courseId <= 0 ||  studentId <= 0)
            {
                return BadRequest("CourseId and/or StudentId should be > 0");
            }
            try
            {
                var enrollment = await _enrollmentService.GetEnrollmentByCourseIdAndStudentId(courseId,studentId);
                if (enrollment == null)
                {
                    return NotFound("No enrollment is available");
                }

                return Ok(enrollment);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }


    }
}
