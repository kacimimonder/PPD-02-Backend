using Application.DTOs.CourseModule;
using Application.DTOs.Enrollment;
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
    public class CourseModuleController : ControllerBase
    {
        private CourseModuleService _courseModuleService;
        public CourseModuleController(CourseModuleService courseModuleService)
        {
            this._courseModuleService = courseModuleService;
        }


        [Authorize(Roles = "Instructor")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCourseModuleAsync([FromBody] CourseModuleCreateDTO courseModuleCreateDTO)
        {
            if (courseModuleCreateDTO == null)
            {
                return BadRequest("Course module data is null.");
            }
            try
            {
                int instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                int courseModuleId = await _courseModuleService.CreateCourseModuleAsync(courseModuleCreateDTO, instructorId);
                return Ok(courseModuleId);
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error adding course module: {ex.Message}");
            }
        }



        [Authorize(Roles = "Instructor")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCourseModuleAsync(int id, [FromBody] CourseModuleCreateDTO courseModuleUpdateDTO)
        {
            if (courseModuleUpdateDTO == null)
            {
                return BadRequest("Invalid course module data.");
            }
            try
            {
                int instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _courseModuleService.UpdateCourseModuleAsync(id,courseModuleUpdateDTO,instructorId);
                return NoContent();
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating course module: {ex.Message}");
            }
        }



        [Authorize(Roles = "Instructor")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCourseModuleAsync(int id)
        {
            try
            {
                int instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _courseModuleService.DeleteCourseModuleAsync(id,instructorId);
                return NoContent();
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            //catch (NotFoundException ex)
            //{
            //    return NotFound(ex.Message);
            //}
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting course module: {ex.Message}");
            }
        }



    }
}
