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
