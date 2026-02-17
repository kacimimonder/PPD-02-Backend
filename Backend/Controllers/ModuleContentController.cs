using API.DTO;
using Application.DTOs.ModuleContent;
using Application.Exceptions;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace API.Controllers
{
    [Route("api/moduleContent")]
    [ApiController]
    public class ModuleContentController : ControllerBase
    {
        private readonly ModuleContentService _moduleContentService;

        public ModuleContentController(ModuleContentService moduleContentService)
        {
            _moduleContentService = moduleContentService;
        }



        [Authorize(Roles = "Instructor")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddModuleContentAsync([FromForm] ModuleContentDTO moduleContentDTO)
        {
            if (moduleContentDTO == null || moduleContentDTO.moduleContentCreateDTO == null)
            {
                return BadRequest("Module content data is null.");
            }
            int instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                Stream? videoStream = null;

                if (moduleContentDTO.videoFile != null)
                {
                    videoStream = moduleContentDTO.videoFile.OpenReadStream();
                }
                string fileName = moduleContentDTO?.videoFile?.Name;
                int moduleContentId = await _moduleContentService.AddModuleContentAsync(instructorId, moduleContentDTO.moduleContentCreateDTO,videoStream,fileName);
                return Ok(moduleContentId);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);  
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error adding module content: {ex.Message}");
            }
        }



        [Authorize(Roles = "Instructor")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateModuleContentAsync([FromForm] ModuleContentDTOUpdate moduleContentDTOUpdate)
        {
            if (moduleContentDTOUpdate == null || moduleContentDTOUpdate.ModuleContentUpdateDTO == null)
            {
                return BadRequest("Module content data is null.");
            }

            Stream? videoStream = null;
            int instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                if (moduleContentDTOUpdate.videoFile != null)
                {
                    videoStream = moduleContentDTOUpdate.videoFile.OpenReadStream();
                }

                string fileName = moduleContentDTOUpdate?.videoFile?.FileName;

                await _moduleContentService.UpdateModuleContentAsync(instructorId,
                    moduleContentDTOUpdate.ModuleContentUpdateDTO,
                    videoStream,fileName);

                return Ok("Module content updated successfully");
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating module content: {ex.Message}");
            }
            finally
            {
                videoStream?.Dispose();
            }
        }



        [Authorize(Roles = "Instructor")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteModuleContentAsync(int id)
        {
            try
            {
                int instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _moduleContentService.DeleteModuleContentAsync(instructorId,id);
                return Ok($"Module content with ID {id} deleted successfully.");
            }
            catch (NotFoundException ex)
            {
                return NotFound($"{ex.Message}");
            }
            catch (ForbiddenException ex)
            {
                return BadRequest($"{ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting module content: {ex.Message}");
            }
        }

    }
}
