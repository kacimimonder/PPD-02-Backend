using Application.DTOs.Quiz;
using Application.Exceptions;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student,Instructor")]
    public class QuizProgressController : ControllerBase
    {
        private readonly QuizProgressService _quizProgressService;

        public QuizProgressController(QuizProgressService quizProgressService)
        {
            _quizProgressService = quizProgressService;
        }

        [HttpPost("assignments")]
        [Authorize(Roles = "Instructor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateAssignments([FromBody] QuizAssignmentCreateDto request)
        {
            if (request.QuizId <= 0 || request.EnrollmentIds == null || request.EnrollmentIds.Count == 0)
            {
                return BadRequest("QuizId and EnrollmentIds are required.");
            }

            int instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try
            {
                int createdCount = await _quizProgressService.AssignQuizToStudentsAsync(instructorId, request);
                return Ok(new { createdCount });
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("attempts")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(StudentQuizAttemptReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateAttempt([FromBody] StudentQuizAttemptCreateDto request)
        {
            if (request.QuizId <= 0 || request.EnrollmentId <= 0 || request.TotalQuestions <= 0)
            {
                return BadRequest("QuizId, EnrollmentId and TotalQuestions must be greater than zero.");
            }

            int studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try
            {
                var created = await _quizProgressService.CreateAttemptAsync(studentId, request);
                return Ok(created);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("courses/{courseId:int}")]
        [Authorize(Roles = "Instructor")]
        [ProducesResponseType(typeof(InstructorCourseQuizProgressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCourseProgress(int courseId)
        {
            if (courseId <= 0)
            {
                return BadRequest("Course id must be greater than zero.");
            }

            int instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try
            {
                var progress = await _quizProgressService.GetInstructorCourseProgressAsync(instructorId, courseId);
                return Ok(progress);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }
    }
}
