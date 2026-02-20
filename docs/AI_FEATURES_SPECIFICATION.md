## 1. Recommended Final Scope (Proposed by Copilot)

This section proposes a **doable, high-impact scope** for your university project based on your current codebase and SQL schema.

### Why these features?

- They map directly to your existing data (`Courses`, `CourseModules`, `ModuleContents`, `Enrollments`, `EnrollmentProgresses`).
- They are clearly demoable in UI.
- They avoid heavy ML training early, while still feeling smart and useful.

### Proposed Features We Should Build

#### F1) Module Quiz Generation (Student + Instructor)

**User flow:**

- Student opens a module and clicks **Generate Quiz**.
- System generates 5–10 MCQs from module contents and returns instantly.

**MVP behavior (no schema change):**

- Quiz is generated on demand and returned to frontend.
- Quiz is not permanently saved.

**Phase-2 behavior (with schema change):**

- Instructor can save generated quiz as editable draft.
- Student attempts can be recorded.

---

#### F2) Module Smart Summary (Student)

**User flow:**

- Student clicks **Summarize This Module**.
- System returns concise bullet-point summary from all text content in module.

**MVP behavior:**

- Summarize only text content (`ModuleContents.Content`).
- If only videos exist, return clear fallback message.

---

#### F3) Ask AI About This Module (Grounded Chat)

**User flow:**

- Student asks questions inside module page (e.g., "Explain normalization with examples").
- AI answers using module content context.

**MVP behavior:**

- Prompt includes module content + short conversation history.
- Chat is session-based (frontend memory), not DB persistence yet.

---

#### F4) Personalized Study Coach (Progress-based)

**User flow:**

- Student clicks **My AI Study Plan** on dashboard.
- System uses enrollment/progress to suggest:
  - what to study next,
  - weak areas (low completion),
  - estimated weekly plan.

**MVP behavior:**

- Use rule-based analytics from SQL + LLM for natural-language explanation.
- No ML model training required.

---

## 2. Features We Should NOT Build Now (Yet)

### Skip for now

- Full dropout prediction ML model.
- Full RAG + vector DB pipeline.
- Automatic grading with persistent question banks.

### Why skip now

- Higher implementation risk.
- More infrastructure and data quality work.
- Less predictable timeline for university grading.

We can mention these as **future work** in final report.

---

## 3. Technical Design (Plain Language)

### Architecture

Frontend -> Backend (.NET, authenticated `/api/ai/*`) -> AI microservice (FastAPI) -> LLM provider

### Data usage per feature

- F1/F2/F3 read from `CourseModules` + `ModuleContents`.
- F4 reads from `Enrollments` + `EnrollmentProgresses` + module totals.

### Why this is good

- Backend remains source of truth + auth guard.
- AI service focuses only on prompt logic.
- Easy to change AI provider later.

---

## 4. Staged Build Plan (What we will implement in order)

### Stage A (Already done baseline)

- AI gateway endpoints and microservice scaffold.

### Stage B (First feature set)

1. **Implement F2 Summary by Module ID**
2. **Implement F1 Quiz by Module ID**
3. **Test with real module records**

**Checkpoint B deliverable:**

- Frontend can call backend endpoint with `moduleId` and get summary/quiz.

### Stage C (Second feature set)

1. **Implement F3 grounded module chat**
2. Add token-budgeting and context truncation
3. Add basic safety checks and fallback responses

**Checkpoint C deliverable:**

- Student can ask contextual questions and get grounded responses.

### Stage D (Third feature set)

1. **Implement F4 study coach endpoint**
2. Build SQL aggregation query for student progress signals
3. Generate structured plan output (weekly plan + priorities)

**Checkpoint D deliverable:**

- Student dashboard gets personalized recommendations.

### Stage E (Hardening)

1. Add caching and rate limiting
2. Add request/response logs and usage metrics
3. Document API contracts and failure behavior

---

## 5. API Endpoints to Build (Backend-facing contract)

### Student endpoints

- `POST /api/ai/modules/{moduleId}/summary`
- `POST /api/ai/modules/{moduleId}/quiz`
- `POST /api/ai/modules/{moduleId}/chat`
- `GET /api/ai/students/{studentId}/study-plan`

### Internal behavior

- Backend validates role + ownership/enrollment.
- Backend fetches required DB context.
- Backend calls AI service with normalized payload.
- Backend returns clean response DTO to frontend.

---

## 6. Testing Plan (Must pass before moving stages)

### Stage B tests

- Unit test: module content aggregation logic.
- Integration test: summary/quiz endpoint returns success with valid module.
- Negative test: invalid moduleId returns `404`.

### Stage C tests

- Prompt truncation tests for long module text.
- Safety fallback test when AI provider fails.
- Unauthorized user gets `401/403`.

### Stage D tests

- Progress analytics SQL returns expected completion percentages.
- Study plan endpoint returns deterministic structure.

---

## 7. Backend Integration Instructions (for teammate)

1. Add AI controller routes by **moduleId** and **studentId**.
2. In service layer, add methods to gather:
   - module text blocks,
   - student completion signals.
3. Map responses to DTOs (summary, quiz, chat, study-plan).
4. Enforce role checks and enrollment checks before AI calls.
5. Add appsettings values for AI service timeout/base URL.
6. Add logs around AI latency, failure count, and token/usage metadata.

---

## 8. Decision Request (Approve/Adjust)

Please confirm one of the following:

- **Option 1 (Recommended):** Build exactly F1 + F2 + F3 + F4 in stages B→E.
- **Option 2:** Start smaller with only F1 + F2 first, then decide.
- **Option 3:** Add instructor quiz-saving in Stage B (requires DB schema updates now).

---

## 9. Execution Status (Live)

### Decision

- [x] Option 1 approved by project owner.

### Current stage

- [x] Stage B implemented in backend code.
- [x] Stage B authenticated happy-path test completed.
- [ ] Stage C not started.

### Stage B implemented endpoints

- `POST /api/ai/modules/{moduleId}/summary`
- `POST /api/ai/modules/{moduleId}/quiz`

### Stage B test evidence collected

- Build success: `dotnet build Backend.sln`
- AI health: `GET /api/ai/health` returns `{ "status": "ok" }`
- Security checks:
  - summary endpoint without token => `401`
  - quiz endpoint without token => `401`

### Stage B authenticated evidence

- `CREATE_RESP=User created successfully`
- `LOGIN_USER_ID=4`
- `COURSE_ID=2`
- `MODULE_ID=1`
- `MODULE_CONTENT_RESP=1`
- `SUMMARY_PROVIDER=fake`
- `QUIZ_PROVIDER=fake`

Stage B is now closed and Stage C can start.
