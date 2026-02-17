using Application.DTOs.Course;
using Application.DTOs.Enrollment;
using Application.DTOs.EnrollmentProgress;
using Application.Exceptions;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentProgressController : ControllerBase
    {
        private EnrollmentProgressService _enrollmentProgressService;
        public EnrollmentProgressController(EnrollmentProgressService enrollmentProgressService)
        {
            this._enrollmentProgressService = enrollmentProgressService;
        }



        [Authorize(Roles = "Student")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] EnrollmentProgressCreateDTO request)
        {
            if ( request.ModuleContentId <= 0 || request.EnrollmentId< 0)
                return BadRequest("Verify the data you have entered");

            int studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                await _enrollmentProgressService.CreateEnrollmentProgress(studentId, request);
                return Ok("Enrollment progress created.");
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, $"{ex.Message}");
            }
            catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error adding enrollment progress: {ex.Message}");
            }

        }


    }
}
