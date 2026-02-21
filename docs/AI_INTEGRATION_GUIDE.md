# MiniCoursera AI Integration Guide

## 1) What is implemented now (Stage 1)

We implemented a **hybrid architecture**:

- A standalone **Python FastAPI microservice** for AI features.
- The existing **.NET backend acts as a proxy/gateway** for frontend calls.

### Stage 1 features

- `chat`
- `summary`
- `quiz generation`

### Why this design

- Keeps AI provider logic isolated from core backend business logic.
- Allows provider swaps (Gemini/OpenAI/local model) without frontend/backend contract changes.
- Lets you scale AI service independently.

---

## 2) Architecture (plain language)

Frontend -> .NET Backend (`api/ai/*`) -> Python AI Service -> Gemini

- Frontend never sees provider keys.
- Backend controls auth and API boundaries.
- Python service focuses only on prompt and model logic.

---

## 3) Files added/changed

### Python service

- `Application/Services/aiService/chatbot.py` (converted to FastAPI app)
- `Application/Services/aiService/requirements.txt`
- `Application/Services/aiService/README.txt`
- `Application/Services/aiService/.env.example`

### .NET backend integration

- `Application/Configuration/AiServiceSettings.cs`
- `Application/DTOs/AI/AiChatRequestDto.cs`
- `Application/DTOs/AI/AiSummaryRequestDto.cs`
- `Application/DTOs/AI/AiQuizRequestDto.cs`
- `Application/DTOs/AI/AiTextResponseDto.cs`
- `Application/Services/AiService.cs`
- `Backend/Controllers/AIController.cs`
- `Backend/Program.cs`
- `Backend/appsettings.json`
- `Application/Application.csproj` (logging abstractions package)

---

## 4) Run locally

## 4.1 Start Python AI service

From `Application/Services/aiService`:

1. Create `.env` from `.env.example`.
2. Install dependencies:
   - `python -m pip install -r requirements.txt`
3. Run:
   - `uvicorn chatbot:app --host 127.0.0.1 --port 8001 --reload`

For testing without real Gemini key:

- set `AI_FAKE_MODE=true` in `.env`

Swagger:

- `http://127.0.0.1:8001/docs`

## 4.2 Start .NET backend

From `Backend`:

- `dotnet run --project API.csproj`

The backend reads AI service URL from:

- `Backend/appsettings.json` -> `AiService:BaseUrl`

### 4.3 Preflight required for auth tests

Before testing authenticated AI endpoints, ensure:

1. SQL Server is running and reachable.
2. `Backend/appsettings.json` has a valid `ConnectionStrings:DefaultConnection`.
3. Database schema is applied from `MinCourseraSQLScript.sql`.

If SQL is not connected, login/user creation fails and authenticated tests cannot run.

---

## 4.4 Testing AI with real Gemini (non-fake mode)

Before implementing or extending summarization, quizzes, etc., validate the AI pipeline with a real API key.

### Step 1: Get a Gemini API key

1. Go to [Google AI Studio](https://aistudio.google.com/apikey).
2. Sign in and create an API key (free tier available).
3. Copy the key.

### Step 2: Configure the Python AI service

In `Application/Services/aiService/.env`:

- Set `GEMINI_API_KEY=<your-pasted-key>` (replace the placeholder).
- Set `AI_FAKE_MODE=false` so the service calls Gemini instead of returning fake responses.

Example `.env`:

```env
GEMINI_API_KEY=AIzaSy...
GEMINI_MODEL=gemini-2.5-flash
AI_FAKE_MODE=false
```

### Step 3: Restart the Python AI service

From `Application/Services/aiService`:

```bash
python -m uvicorn chatbot:app --host 127.0.0.1 --port 8001 --reload
```

(On Windows PowerShell use the same command.)

### Step 4: Verify health (real mode)

- **Option A – Python directly:**  
  `GET http://127.0.0.1:8001/health`  
  Response should include `"provider": "gemini"` and `"fake_mode": false`.

- **Option B – Via .NET:**  
  In Swagger, call `GET /api/ai/health`.  
  You should get `200` and the backend will have proxied the Python health (Python must return `"status": "ok"` for the backend to return 200).

### Step 5: Test chat, summary, and quiz via Swagger

1. Log in: `POST /api/User/login` and copy the `token` from the response.
2. In Swagger, click **Authorize**, paste the token, then **Authorize** / **Close**.
3. Call these endpoints with the examples below.

**POST /api/ai/chat**

```json
{
  "message": "What is object-oriented programming in one sentence?",
  "language": "en",
  "history": []
}
```

Expected: `200` with `output` containing a short explanation and `provider`: `"gemini"`.

**POST /api/ai/summary**

```json
{
  "text": "Object-oriented programming (OOP) is a programming paradigm based on the concept of objects, which can contain data and code. The four pillars are encapsulation, abstraction, inheritance, and polymorphism. Encapsulation bundles data and methods; abstraction hides complexity; inheritance allows code reuse; polymorphism allows different behaviors under the same interface.",
  "maxBullets": 5,
  "language": "en"
}
```

Expected: `200` with `output` containing bullet points and `provider`: `"gemini"`.

**POST /api/ai/quiz**

```json
{
  "text": "SQL is a standard language for managing relational databases. SELECT retrieves data, INSERT adds rows, UPDATE modifies rows, DELETE removes rows. WHERE filters rows; JOIN combines tables. Primary keys uniquely identify rows; foreign keys link tables.",
  "questionsCount": 3,
  "language": "en"
}
```

Expected: `200` with `output` containing quiz questions (often JSON) and `provider`: `"gemini"`.

If all three return `200` with `provider: "gemini"`, the AI API is working in non-fake mode and you can proceed to implement/refine summarization, quizzes, and other AI features.

---

## 5) API contracts

## 5.1 Backend endpoints (frontend should use these)

- `GET /api/ai/health` (anonymous)
- `POST /api/ai/chat` (requires auth)
- `POST /api/ai/summary` (requires auth)
- `POST /api/ai/quiz` (requires auth)

### Example chat request body

```json
{
  "message": "Explain recursion simply",
  "language": "en",
  "history": [{ "role": "user", "content": "I struggle with recursion" }]
}
```

### Example response shape

```json
{
  "output": "...",
  "provider": "gemini",
  "model": "gemini-2.5-flash"
}
```

---

## 6) Validation performed

- Built backend successfully:
  - `dotnet build Backend.sln`
- Validated Python service endpoints:
  - `GET /health`
  - `POST /summary`
- Validated backend integration:
  - `GET /api/ai/health` -> `{"status":"ok"}`
- Validated security:
  - `POST /api/ai/chat` without token -> `401`

---

## 7) Stage roadmap (for graded project)

## Stage B status (implemented)

### What was built

- Added module-aware AI orchestration service:
  - `Application/Services/AiModuleService.cs`
- Added module request DTOs:
  - `Application/DTOs/AI/AiModuleSummaryRequestDto.cs`
  - `Application/DTOs/AI/AiModuleQuizRequestDto.cs`
- Extended repository contract and implementation to load module context:
  - `Domain/Interfaces/ICourseModuleRepository.cs`
  - `Infrastructure/Repositories/CourseModuleRepository.cs`
- Added backend endpoints:
  - `POST /api/ai/modules/{moduleId}/summary`
  - `POST /api/ai/modules/{moduleId}/quiz`
  - in `Backend/Controllers/AIController.cs`

### How it works (plain language)

1. Backend receives moduleId and user token.
2. Backend reads user id + role from JWT claims.
3. Backend loads module with contents from DB.
4. Authorization rules:
   - Student must be enrolled in the module's course.
   - Instructor must own the module's course.
5. Backend builds text context from module title, description, and each section's name and content.
6. Backend prepends a **module-specific instruction** (e.g. "Context: The following is a single course module. Summarize only this module.") so the AI responds in a module-scoped way.
7. Backend calls the same AI microservice `/summary` and `/quiz` with this combined text.

**Request body (optional):** You can send `{}` for defaults, or e.g. `{ "maxBullets": 5, "language": "en" }` for summary and `{ "questionsCount": 5, "language": "en" }` for quiz.

### Stage B tests executed

- Build verification:
  - `dotnet build Backend.sln` (success)
- Service health:
  - `GET /api/ai/health` -> `{"status":"ok"}`
- Security checks on new endpoints:
  - `POST /api/ai/modules/1/summary` without token -> `401`
  - `POST /api/ai/modules/1/quiz` without token -> `401`

### Stage B authenticated happy-path (completed)

Executed full real flow:

1. Created instructor user.
2. Logged in and obtained JWT.
3. Created course (after ensuring `Language` and `Subject` lookup rows exist).
4. Created course module and module content.
5. Called AI module endpoints with JWT.

Observed results:

- `CREATE_RESP=User created successfully`
- `LOGIN_USER_ID=4`
- `COURSE_ID=2`
- `MODULE_ID=1`
- `MODULE_CONTENT_RESP=1`
- `SUMMARY_PROVIDER=fake`
- `QUIZ_PROVIDER=fake`

Conclusion: Stage B is fully validated end-to-end.

## Stage C status (implemented)

### What was built

- Added module-grounded chat endpoint:
  - `POST /api/ai/modules/{moduleId}/chat`
  - in `Backend/Controllers/AIController.cs`
- Added module chat request DTO:
  - `Application/DTOs/AI/AiModuleChatRequestDto.cs`
- Extended module AI service for grounded chat:
  - `Application/Services/AiModuleService.cs`

### Stage C technical behavior

1. Backend validates JWT + role and module access.
2. Backend builds grounded module context from title/description/content.
3. Backend truncates context (`MaxModuleContextChars = 12000`) to control token size.
4. Backend trims history (`MaxHistoryMessages = 12`) to control prompt growth.
5. Backend sends grounded prompt to AI service.
6. If AI provider is unavailable, backend returns graceful fallback response (`provider=backend-fallback`).

### Stage C tests executed

- Unauthorized test:
  - `POST /api/ai/modules/1/chat` without token -> `401`
- Authenticated happy-path:
  - `CREATE_RESP=User created successfully`
  - `LOGIN_USER_ID=5`
  - `COURSE_ID=3`
  - `MODULE_ID=2`
  - `MODULE_CONTENT_RESP=2`
  - `CHAT_PROVIDER=fake`
  - `CHAT_MODEL=local-test`
- Provider-down fallback test:
  - stopped AI microservice
  - called module chat on owned module
  - response contained:
    - `FALLBACK_PROVIDER=backend-fallback`
    - `FALLBACK_MODEL=n/a`

Conclusion: Stage C is fully validated end-to-end.

---

## Stage 2 (recommended next)

- Add persistence for AI interactions (prompt/response metadata)
- Add prompt templates per course/module type
- Add retry/circuit-breaker logic in .NET proxy
- Add rate limiting and request size limits

## Stage 3 (RAG)

- Add embedding pipeline for course content
- Add vector database (e.g., pgvector, Qdrant, or Azure AI Search)
- Upgrade summary/quiz to retrieval-grounded prompts

## Stage 4 (ML features)

- Dropout prediction model with explainability
- Learning-path recommendations using enrollment and progress signals

---

## 8) Security and production notes

- Keep provider API keys only in AI service env variables.
- Do not expose Python service publicly unless behind gateway/network controls.
- Add structured logging + correlation IDs for tracing.
- Add secret management (Azure Key Vault or similar) for deployment.

---

## 9) Do you need SQL schema now?

For Stage 1, **DTOs/models are enough**.

For Stage 2+ (recommendations/dropout/analytics), SQL schema is very useful because we need:

- reliable joins for user-course-progress data,
- feature engineering definitions,
- clear constraints and indexing for training/serving queries.

So: not required for what is already built, but strongly recommended before ML stages.
