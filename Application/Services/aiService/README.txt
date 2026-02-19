MiniCoursera AI service (Stage 1)

What this service provides:
- Chat endpoint
- Summary endpoint
- Quiz generation endpoint

Setup:
1) Create `.env` in this folder with:
	- `GEMINI_API_KEY=your_key_here`
	- Optional: `GEMINI_MODEL=gemini-2.5-flash`
	- Optional: `AI_FAKE_MODE=true` (for local testing without API key)

2) Install dependencies:
	python -m pip install -r requirements.txt

3) Run service:
	uvicorn chatbot:app --host 0.0.0.0 --port 8001 --reload

4) Open Swagger docs:
	http://localhost:8001/docs

API endpoints:
- GET /health
- POST /chat
- POST /summary
- POST /quiz

Notes:
- The .NET backend should call this service instead of calling LLM APIs directly.
- Keep this API key only in the Python service `.env` and never in frontend code.