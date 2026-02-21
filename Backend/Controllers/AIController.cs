using Application.DTOs.AI;
using Application.Exceptions;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/ai")]
    [ApiController]
    [Authorize(Roles = "Student,Instructor")]
    public class AIController : ControllerBase
    {
        private readonly AiService _aiService;
        private readonly AiModuleService _aiModuleService;

        public AIController(AiService aiService, AiModuleService aiModuleService)
        {
            _aiService = aiService;
            _aiModuleService = aiModuleService;
        }

        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Health(CancellationToken cancellationToken)
        {
            var isHealthy = await _aiService.IsHealthyAsync(cancellationToken);
            if (isHealthy)
            {
                return Ok(new { status = "ok" });
            }

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "unavailable" });
        }

        [HttpPost("chat")]
        [ProducesResponseType(typeof(AiTextResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Chat([FromBody] AiChatRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _aiService.ChatAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, ex.Message);
            }
        }

        [HttpPost("summary")]
        [ProducesResponseType(typeof(AiTextResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Summary([FromBody] AiSummaryRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _aiService.SummarizeAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, ex.Message);
            }
        }

        [HttpPost("quiz")]
        [ProducesResponseType(typeof(AiTextResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Quiz([FromBody] AiQuizRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _aiService.GenerateQuizAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, ex.Message);
            }
        }

        [HttpPost("modules/{moduleId:int}/summary")]
        [ProducesResponseType(typeof(AiTextResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> SummaryByModule(
            int moduleId,
            [FromBody] AiModuleSummaryRequestDto? request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUser(out var userId, out var role, out var unauthorizedResult))
            {
                return unauthorizedResult;
            }

            var req = request ?? new AiModuleSummaryRequestDto();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _aiModuleService.GenerateModuleSummaryAsync(moduleId, userId, role, req, cancellationToken);
                return Ok(response);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, ex.Message);
            }
        }

        [HttpPost("modules/{moduleId:int}/quiz")]
        [ProducesResponseType(typeof(AiTextResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> QuizByModule(
            int moduleId,
            [FromBody] AiModuleQuizRequestDto? request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUser(out var userId, out var role, out var unauthorizedResult))
            {
                return unauthorizedResult;
            }

            var req = request ?? new AiModuleQuizRequestDto();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _aiModuleService.GenerateModuleQuizAsync(moduleId, userId, role, req, cancellationToken);
                return Ok(response);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, ex.Message);
            }
        }

        [HttpPost("modules/{moduleId:int}/chat")]
        [ProducesResponseType(typeof(AiTextResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> ChatByModule(
            int moduleId,
            [FromBody] AiModuleChatRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUser(out var userId, out var role, out var unauthorizedResult))
            {
                return unauthorizedResult;
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _aiModuleService.ChatOnModuleAsync(moduleId, userId, role, request, cancellationToken);
                return Ok(response);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, ex.Message);
            }
        }

        private bool TryGetCurrentUser(out int userId, out string role, out IActionResult unauthorizedResult)
        {
            unauthorizedResult = Unauthorized("Missing or invalid auth claims.");
            userId = 0;
            role = string.Empty;

            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            if (!int.TryParse(idValue, out userId) || string.IsNullOrWhiteSpace(role))
            {
                return false;
            }

            return true;
        }
    }
}
