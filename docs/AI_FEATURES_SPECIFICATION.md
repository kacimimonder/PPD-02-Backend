# MiniCoursera AI Features Specification

## 1) Project Goal

Deliver practical AI features inside MiniCoursera that are:

- useful for students in daily learning,
- simple to demo in class,
- feasible with current architecture and timeline.

---

## 2) Implemented Features (Current)

### F1. Module Quiz Generation

**User value**

- Students can generate quick practice questions from the current module.

**Behavior**

- Reads module text content and generates quiz output on demand.
- Does not persist quiz attempts in the database (MVP choice).

**Endpoint**

- `POST /api/ai/modules/{moduleId}/quiz`

---

### F2. Module Smart Summary

**User value**

- Students get concise, readable summaries before revision or exams.

**Behavior**

- Uses module title/description/content as context.
- Returns concise bullet-style text response.

**Endpoint**

- `POST /api/ai/modules/{moduleId}/summary`

---

### F3. Grounded Module Chat

**User value**

- Students can ask natural-language questions while staying inside the module context.

**Behavior**

- Uses module context + limited recent chat history.
- Applies prompt-size controls to keep responses stable.
- Supports server-side conversation memory via `conversationId`.
- Normalizes conversation history (role/content formatting) before AI calls.
- Returns graceful fallback if the AI provider is unavailable.

**Endpoint**

- `POST /api/ai/modules/{moduleId}/chat`

**Operational support**

- `GET /api/ai/monitoring` (Instructor) provides call/error/latency snapshots for AI endpoints.

---

### Foundation Capability

- `GET /api/ai/health` checks whether AI service is reachable.

---

## 3) Architecture (Non-Technical Summary)

Frontend -> Backend (.NET API) -> AI Microservice (FastAPI/Python) -> LLM Provider

**Why this architecture works for the project**

- Backend keeps authentication and authorization centralized.
- AI provider keys stay outside frontend.
- AI logic is isolated and can be changed without breaking app APIs.

---

## 4) Security and Access Rules

- AI endpoints require authenticated users.
- Allowed roles: Student and Instructor.
- Students can use module AI only if enrolled in that course.
- Instructors can use module AI only for courses they own.

---

## 5) Validation Summary (Evidence)

### Build and service checks

- `dotnet build Backend.sln` passed.
- `GET /api/ai/health` returned service available.

### Security checks

- Protected AI endpoints without token returned `401`.

### Authenticated happy-path checks

- Created user, logged in, created course/module/content, then called AI endpoints.
- Verified successful responses from summary/quiz/chat endpoints.

### Resilience checks

- When AI provider was unavailable, module chat returned a controlled fallback response.

### Orchestration and observability checks

- Build passed after adding server-side memory + history normalization.
- Build passed after adding structured logging + monitoring service.

---

## 6) Team Contribution Mapping

### Moundir

- Implemented initial chatbot/microservice foundation.
- Co-authored and maintained AI feature specification and scope decisions.

### Akram

- Implemented quiz generation feature integration.
- Executed and validated HTTP endpoint/URL testing for AI routes.

### Islem

- Implemented summary feature integration.
- Suggested additional contribution: grounded chat experience tuning (context formatting + safe fallback behavior).

---

## 7) Next Planned Feature (Stage D)

### F4. Personalized Study Plan (Planned)

**Target endpoint**

- `GET /api/ai/students/{studentId}/study-plan`

**Planned behavior**

- Use enrollment and progress signals to suggest:
  - what to study next,
  - weak areas,
  - weekly study priorities.

**Status**

- Not started yet.

---

## 8) Out-of-Scope for Current MVP

- Full ML dropout prediction.
- Full RAG/vector database pipeline.
- Persistent AI grading workflows.

These remain suitable as future work after Stage D/E.
