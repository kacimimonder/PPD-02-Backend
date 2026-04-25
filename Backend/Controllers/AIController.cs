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
        private readonly UserService _userService;
        private readonly CourseService _courseService;
        private readonly EnrollmentService _enrollmentService;

        public AIController(
            AiService aiService,
            AiModuleService aiModuleService,
            UserService userService,
            CourseService courseService,
            EnrollmentService enrollmentService)
        {
            _aiService = aiService;
            _aiModuleService = aiModuleService;
            _userService = userService;
            _courseService = courseService;
            _enrollmentService = enrollmentService;
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
                return Ok(new { status = "ok", message = "AI service is operational." });
            }

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "unavailable",
                message = "The AI service is currently unavailable. Features like quiz generation, summaries, and chat will use fallback responses until the service is restored."
            });
        }

        [HttpGet("monitoring")]
        [Authorize(Roles = "Instructor")]
        [ProducesResponseType(typeof(Dictionary<string, AiMonitoringService.AiMonitoringSnapshot>), StatusCodes.Status200OK)]
        public IActionResult Monitoring()
        {
            var snapshot = _aiService.GetMonitoringSnapshot();
            return Ok(snapshot);
        }

        [HttpPost("chat")]
        [ProducesResponseType(typeof(AiTextResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Chat([FromBody] AiChatRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CreateValidationError(ModelState));
            }

            try
            {
                var response = await _aiService.ChatAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, CreateErrorResponse("ai_unavailable", ex.Message));
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
                return BadRequest(CreateValidationError(ModelState));
            }

            try
            {
                var response = await _aiService.SummarizeAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, CreateErrorResponse("ai_unavailable", ex.Message));
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
                return BadRequest(CreateValidationError(ModelState));
            }

            try
            {
                var response = await _aiService.GenerateQuizAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, CreateErrorResponse("ai_unavailable", ex.Message));
            }
        }

        [HttpPost("sentiment")]
        [ProducesResponseType(typeof(AiSentimentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Sentiment([FromBody] AiSentimentRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CreateValidationError(ModelState));
            }

            var response = await _aiService.AnalyzeSentimentAsync(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("emotion")]
        [ProducesResponseType(typeof(AiEmotionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Emotion([FromBody] AiEmotionRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CreateValidationError(ModelState));
            }

            var response = await _aiService.AnalyzeEmotionAsync(request, cancellationToken);
            return Ok(response);
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
                return BadRequest(CreateValidationError(ModelState));
            }

            try
            {
                var response = await _aiModuleService.GenerateModuleSummaryAsync(moduleId, userId, role, req, cancellationToken);
                return Ok(response);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(CreateErrorResponse("validation_error", ex.Message));
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, CreateErrorResponse("access_denied", ex.Message));
            }
            catch (NotFoundException ex)
            {
                return NotFound(CreateErrorResponse("not_found", ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, CreateErrorResponse("ai_unavailable", ex.Message));
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
                return BadRequest(CreateValidationError(ModelState));
            }

            try
            {
                var response = await _aiModuleService.GenerateModuleQuizAsync(moduleId, userId, role, req, cancellationToken);
                return Ok(response);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(CreateErrorResponse("validation_error", ex.Message));
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, CreateErrorResponse("access_denied", ex.Message));
            }
            catch (NotFoundException ex)
            {
                return NotFound(CreateErrorResponse("not_found", ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, CreateErrorResponse("ai_unavailable", ex.Message));
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
                return BadRequest(CreateValidationError(ModelState));
            }

            try
            {
                var response = await _aiModuleService.ChatOnModuleAsync(moduleId, userId, role, request, cancellationToken);
                return Ok(response);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(CreateErrorResponse("validation_error", ex.Message));
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, CreateErrorResponse("access_denied", ex.Message));
            }
            catch (NotFoundException ex)
            {
                return NotFound(CreateErrorResponse("not_found", ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, CreateErrorResponse("ai_unavailable", ex.Message));
            }
        }

        [HttpGet("recommendations/profile")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(AiRecommendationsProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRecommendationsProfile()
        {
            if (!TryGetCurrentUser(out var userId, out _, out var unauthorizedResult))
            {
                return unauthorizedResult;
            }

            var profile = await _userService.GetAiRecommendationsProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPost("recommendations")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(AiCourseRecommendationsResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RecommendCourses([FromBody] AiRecommendCoursesRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CreateValidationError(ModelState));
            }

            if (!TryGetCurrentUser(out var userId, out _, out var unauthorizedResult))
            {
                return unauthorizedResult;
            }

            var ambitions = request.Ambitions.Trim();
            var interests = request.Interests.Trim();
            if (string.IsNullOrWhiteSpace(ambitions) || string.IsNullOrWhiteSpace(interests))
            {
                return BadRequest(CreateErrorResponse("validation_error", "Please provide both ambitions and interests."));
            }

            await _userService.UpsertAiRecommendationsProfileAsync(userId, ambitions, interests);

            var discoverCourses = await _courseService.GetDiscoverCoursesAsync(80);
            var enrolled = await _enrollmentService.GetEnrolledCoursesByStudentId(userId);
            var enrolledCourseIds = enrolled.Select(e => e.CourseID).ToHashSet();

            var availableCourses = discoverCourses
                .Where(c => !enrolledCourseIds.Contains(c.Id))
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();

            if (availableCourses.Count == 0)
            {
                return Ok(new AiCourseRecommendationsResultDto
                {
                    Summary = "You are already enrolled in all currently available suggested courses.",
                    Courses = new List<AiRecommendedCourseCardDto>(),
                    Provider = "backend",
                    Model = "n/a",
                    IsFallback = false,
                    Status = "success"
                });
            }

            var aiRequest = new AiCourseRecommendationsRequestDto
            {
                Ambitions = ambitions,
                Interests = interests,
                MaxRecommendations = request.MaxRecommendations,
                Language = string.IsNullOrWhiteSpace(request.Language) ? "en" : request.Language,
                Courses = availableCourses.Select(c => new AiCourseCandidateDto
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    InstructorName = c.Instructor?.FullName ?? string.Empty,
                    ImageUrl = c.ImageUrl,
                    Price = c.Price
                }).ToList()
            };

            var aiResponse = await _aiService.RecommendCoursesAsync(aiRequest, cancellationToken);
            var courseById = availableCourses.ToDictionary(c => c.Id, c => c);

            var cards = aiResponse.Recommendations
                .Where(item => courseById.ContainsKey(item.CourseId))
                .Select(item =>
                {
                    var course = courseById[item.CourseId];
                    return new AiRecommendedCourseCardDto
                    {
                        CourseId = course.Id,
                        Title = course.Title,
                        ImageUrl = course.ImageUrl,
                        Price = course.Price,
                        InstructorName = course.Instructor?.FullName ?? string.Empty,
                        InstructorImageUrl = course.Instructor?.PhotoUrl ?? string.Empty,
                        Reason = item.Reason,
                        MatchScore = item.MatchScore
                    };
                })
                .ToList();

            return Ok(new AiCourseRecommendationsResultDto
            {
                Summary = aiResponse.Summary,
                Courses = cards,
                Provider = aiResponse.Provider,
                Model = aiResponse.Model,
                IsFallback = aiResponse.IsFallback,
                Status = aiResponse.Status
            });
        }

        private bool TryGetCurrentUser(out int userId, out string role, out IActionResult unauthorizedResult)
        {
            unauthorizedResult = Unauthorized(CreateErrorResponse("auth_error",
                "Your session has expired or is invalid. Please sign in again to use AI features."));
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

        private static object CreateErrorResponse(string code, string message)
        {
            return new { error = code, message };
        }

        private static object CreateValidationError(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
        {
            var errors = modelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(err => err.ErrorMessage).ToArray());

            return new
            {
                error = "validation_error",
                message = "Please check your input and try again.",
                details = errors
            };
        }
    }
}
