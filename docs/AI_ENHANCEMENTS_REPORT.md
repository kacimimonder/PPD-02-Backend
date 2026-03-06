# AI Enhancement Report — MiniCoursera Backend

## Overview

This document details all AI-side enhancements implemented across the .NET backend, covering performance optimization, error handling, chat grounding, UX improvements, and test infrastructure.

---

## 1. Latency & Performance Optimizations

### Per-Endpoint Timeouts
Each AI endpoint now has its own configurable timeout (in `appsettings.json`):

| Endpoint | Timeout | Rationale |
|----------|---------|-----------|
| Quiz     | 45s     | Quiz generation is computationally heavier |
| Chat     | 30s     | Conversational responses should be fast |
| Summary  | 30s     | Summaries are moderate complexity |
| Health   | 10s     | Health checks must be snappy |

Previously, all endpoints shared a single hardcoded timeout.

### Polly Retry with Exponential Backoff
Added `Polly 8.5.2` and `Microsoft.Extensions.Http.Polly 8.0.0` to the HTTP client pipeline:
- Retries transient HTTP errors (5xx, 408, network failures)
- Exponential backoff: `RetryBaseDelayMs × 2^(attempt-1)` (default: 500ms, 1000ms)
- Default 2 retries (configurable via `AiService:RetryCount`)
- Each retry attempt is logged

### Timeout Cancellation
Per-request `CancellationTokenSource` with `CancelAfter` ensures requests don't hang:
- Creates a linked token combining the caller's cancellation token with a per-request timeout
- `OperationCanceledException` caught and converted to a friendly fallback response

**Files Modified:**
- `Application/Configuration/AiServiceSettings.cs` — Added timeout & retry settings
- `Application/Services/AiService.cs` — Per-endpoint timeout, CTS, retry-aware error handling
- `Backend/Program.cs` — Polly retry policy registration
- `Backend/appsettings.json` — New configuration values

---

## 2. Error Handling & User-Friendly Responses

### Structured Error Responses
All AI controller errors now return consistent JSON:
```json
{
  "error": "error_code",
  "message": "Human-readable explanation"
}
```

Error codes: `validation_error`, `access_denied`, `not_found`, `ai_unavailable`, `auth_error`

### Validation errors include field details:
```json
{
  "error": "validation_error",
  "message": "Please check the highlighted fields and try again.",
  "details": {
    "fieldName": ["Error description"]
  }
}
```

### HTTP Status → Friendly Message Mapping
The `AiService` maps Python AI service HTTP errors to clear messages:
- **429** → "The AI service is currently busy. Please wait a moment and try again."
- **503** → "The AI service is temporarily unavailable for maintenance. Please try again shortly."
- **504** → "The AI service took too long to respond. Please try a simpler request."
- **500+** → "Something went wrong with the AI service. Our team has been notified."
- **422** → "The AI service couldn't process this request. Please rephrase and try again."

### Auth Error Message
```
"Your session has expired or is invalid. Please sign in again to use AI features."
```

**Files Modified:**
- `Backend/Controllers/AIController.cs` — Structured error responses
- `Application/Services/AiService.cs` — HTTP status mapping

---

## 3. Chat Grounding Refinement

### System Prompt Rewrite
The `BuildGroundedChatSystemPrompt` method was completely rewritten with:

1. **Strict grounding rules**: "You MUST ONLY use the provided module context to answer. Do NOT use any external knowledge, training data, or make assumptions."
2. **Section-aware context**: Lists all available section names from module content
3. **Teaching style**: Designed for educational responses — encourages examples, analogies, step-by-step explanations
4. **Formatting rules**: Markdown with bold, code blocks, max 500-word responses
5. **Off-topic handling**: When asked about unrelated topics, the AI lists available sections and redirects

### Grounded User Message
`BuildGroundedChatUserMessage` now includes: "Use ONLY the module context below to answer. Do not use any external knowledge."

### Prompt Injection Protection
9 new injection patterns added to the detection list:
- "ignore all instructions"
- "forget your instructions"
- "override system prompt"
- "act as if you"
- "pretend you are"
- "bypass content filter"
- "ignore safety"
- "reveal your prompt"
- "show me your system"

All return 400 Bad Request with: `"Your message contains content that isn't allowed. Please rephrase your question about the module content."`

**Files Modified:**
- `Application/Services/AiModuleService.cs` — Grounding prompts, injection patterns

---

## 4. Fallback & UX Improvements

### Context-Aware Fallback Responses

**`CreateSafeFallbackResponse`** now generates markdown-formatted, module-specific fallbacks:
```markdown
## ⚠️ AI Temporarily Unavailable

I'm sorry, but I'm unable to process your request for **{ModuleName}** right now.

**What you can do:**
- **Try again** in a few moments
- **Review the module content** directly
- **Check your connection** and retry

_The AI features will be back shortly. Thank you for your patience!_
```

Sets `IsFallback = true` and `Status = "fallback"`.

**`CreateGroundedFallbackResponse`** lists up to 5 available section names:
```
I can only help with topics covered in this module. Available sections include:
• Section 1
• Section 2
...
Please ask about one of these topics!
```

### Response Metadata
Every AI response now includes:
- `DurationMs` — Processing time in milliseconds
- `IsFallback` — Whether a fallback response was used
- `Status` — One of: `"success"`, `"fallback"`, `"partial"`

**Files Modified:**
- `Application/DTOs/AI/AiTextResponseDto.cs` — New metadata fields
- `Application/Services/AiModuleService.cs` — Fallback methods, timing

---

## 5. Monitoring Enhancements

### New Metrics
The `AiMonitoringService` singleton now tracks:

| Metric | Description |
|--------|-------------|
| `MinDurationMs` | Fastest response time |
| `MaxDurationMs` | Slowest response time |
| `P95DurationMs` | 95th percentile latency |
| `P99DurationMs` | 99th percentile latency |
| `RequestsPerMinute` | Rate calculation since first call |

Percentile calculation maintains a rolling window of the last 1,000 call durations per endpoint.

**Files Modified:**
- `Application/Services/AiMonitoringService.cs`

---

## 6. Load & Performance Test Suite

### Script: `scripts/ai_load_test.ps1`

A comprehensive PowerShell test suite with 9 phases:

| Phase | Description |
|-------|------------|
| 1 | Health check |
| 2 | Authentication (register + login via correct API contracts) |
| 3 | Module discovery (find a module with content) |
| 4 | Health endpoint load test |
| 5 | Quiz generation load test (randomized difficulty) |
| 6 | Summary generation load test (randomized mode) |
| 7 | Chat load test + 5-turn conversation continuity test |
| 8 | Error handling validation (6 test cases) |
| 9 | Monitoring snapshot retrieval |

### Error Handling Test Cases
1. Invalid module ID → expects 404
2. No auth token → expects 401
3. Prompt injection attempt → expects 400
4. Empty message → expects 400
5. Too-short message → expects 400
6. Invalid quiz count (100) → expects 400

### Statistics Computed Per Endpoint
- Total / Success / Failed requests
- Error rate percentage
- Average / Min / Max duration
- P95 / P99 latency percentiles

### Usage
```powershell
# Full test with database
.\ai_load_test.ps1 -BaseUrl "http://localhost:5159" -ConcurrentUsers 5 -RequestsPerUser 3

# Skip module tests (no DB available)
.\ai_load_test.ps1 -BaseUrl "http://localhost:5159" -SkipModuleTests

# Custom credentials
.\ai_load_test.ps1 -StudentEmail "test@example.com" -StudentPassword "Pass123!"
```

### Graceful Degradation
The script automatically adapts when infrastructure is unavailable:
- No SQL Server → skips auth-dependent phases, runs health tests only
- No Python AI service → measures fallback behavior, records 503 responses
- Auto-generates timestamped result files in `scripts/`

---

## 7. Test Results (Infrastructure-Limited Run)

**Environment:** Backend server running, SQL Server unavailable, Python AI service unavailable.

| Endpoint | Requests | Success | Failed | Avg (ms) | P95 (ms) | P99 (ms) | Error Rate |
|----------|----------|---------|--------|----------|----------|----------|------------|
| HEALTH   | 6        | 0       | 6      | 10,009   | 10,013   | 10,013   | 100%       |

- All 503 responses are expected — the Python AI microservice was not running
- The ~10s response time reflects the configured `HealthTimeoutSeconds: 10`
- Auth, module, quiz, summary, chat, and error handling phases were gracefully skipped

**Full infrastructure test** requires:
1. SQL Server with `MiniCourseraDb` database
2. Python FastAPI AI microservice running on port 8001
3. Then run: `.\ai_load_test.ps1 -BaseUrl "http://localhost:5159" -ConcurrentUsers 5 -RequestsPerUser 3`

---

## 8. Configuration Reference

```json
"AiService": {
  "BaseUrl": "http://localhost:8001",
  "TimeoutSeconds": 30,
  "RetryCount": 2,
  "RetryBaseDelayMs": 500,
  "QuizTimeoutSeconds": 45,
  "ChatTimeoutSeconds": 30,
  "SummaryTimeoutSeconds": 30,
  "HealthTimeoutSeconds": 10
}
```

---

## Files Modified Summary

| File | Changes |
|------|---------|
| `Application/Configuration/AiServiceSettings.cs` | Per-endpoint timeouts, retry config |
| `Application/DTOs/AI/AiTextResponseDto.cs` | DurationMs, IsFallback, Status fields |
| `Application/Services/AiMonitoringService.cs` | P95/P99, min/max, requests/minute |
| `Application/Services/AiService.cs` | Timeouts, CTS, friendly errors, retry handling |
| `Application/Services/AiModuleService.cs` | Grounding, fallbacks, injection patterns, timing |
| `Backend/Controllers/AIController.cs` | Structured JSON errors with codes |
| `Backend/Program.cs` | Polly retry policy |
| `Backend/appsettings.json` | New timeout & retry config values |
| `scripts/ai_load_test.ps1` | **NEW** — Load & performance test suite |
| `docs/AI_ENHANCEMENTS_REPORT.md` | **NEW** — This document |
