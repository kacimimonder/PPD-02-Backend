import json
import os
from typing import List, Optional

from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field

load_dotenv()

try:
    from google import genai
except Exception:
    genai = None


class ChatMessage(BaseModel):
    role: str = Field(description="user|assistant")
    content: str = Field(min_length=1, max_length=4000)


class ChatRequest(BaseModel):
    message: str = Field(min_length=1, max_length=4000)
    history: Optional[List[ChatMessage]] = Field(default_factory=list)
    language: str = Field(default="en")


class SummaryRequest(BaseModel):
    text: str = Field(min_length=10, max_length=20000)
    max_bullets: int = Field(default=5, ge=3, le=12)
    language: str = Field(default="en")


class QuizRequest(BaseModel):
    text: str = Field(min_length=30, max_length=20000)
    questions_count: int = Field(default=5, ge=3, le=10)
    language: str = Field(default="en")


class AiTextResponse(BaseModel):
    output: str
    provider: str
    model: str


class HealthResponse(BaseModel):
    status: str
    provider: str
    fake_mode: bool


API_KEY = os.getenv("GEMINI_API_KEY", "")
MODEL_NAME = os.getenv("GEMINI_MODEL", "gemini-2.5-flash")
FAKE_MODE = os.getenv("AI_FAKE_MODE", "false").lower() == "true"

# Log mode at startup so you can confirm in the terminal
import sys
print(f"[AI Service] FAKE_MODE={FAKE_MODE}, GEMINI_API_KEY set={bool(API_KEY)}", file=sys.stderr)

app = FastAPI(
    title="MiniCoursera AI Service",
    version="1.0.0",
    description="AI microservice for chat, summaries, and quiz generation",
)


def _extract_text(response) -> Optional[str]:
    if hasattr(response, "text") and response.text:
        return response.text

    data = None
    try:
        if hasattr(response, "to_json_dict"):
            data = response.to_json_dict()
        elif hasattr(response, "dict"):
            data = response.dict()
    except Exception:
        data = None

    def _find_text(obj):
        if isinstance(obj, dict):
            for key, value in obj.items():
                if key == "text" and isinstance(value, str):
                    return value
                nested = _find_text(value)
                if nested:
                    return nested
        elif isinstance(obj, list):
            for item in obj:
                nested = _find_text(item)
                if nested:
                    return nested
        return None

    return _find_text(data) if data else None


def _fake_response(prompt: str) -> str:
    normalized = " ".join(prompt.split())
    snippet = normalized[:500]
    return (
        "[FAKE_MODE RESPONSE]\n"
        "This is a local deterministic response for testing integration.\n"
        f"Prompt snippet: {snippet}"
    )


def _generate(prompt: str) -> AiTextResponse:
    if FAKE_MODE:
        return AiTextResponse(output=_fake_response(prompt), provider="fake", model="local-test")

    if not API_KEY or genai is None:
        raise HTTPException(
            status_code=500,
            detail="Gemini provider not configured. Set GEMINI_API_KEY or enable AI_FAKE_MODE=true.",
        )

    try:
        client = genai.Client(api_key=API_KEY)
        response = client.models.generate_content(model=MODEL_NAME, contents=prompt)
        text = _extract_text(response)
        if not text:
            raise HTTPException(status_code=502, detail="No text returned by AI provider")
        return AiTextResponse(output=text, provider="gemini", model=MODEL_NAME)
    except HTTPException:
        raise
    except Exception as ex:
        raise HTTPException(status_code=502, detail=f"AI provider call failed: {str(ex)}")


@app.get("/health", response_model=HealthResponse)
def health_check():
    return HealthResponse(
        status="ok",
        provider="fake" if FAKE_MODE else "gemini",
        fake_mode=FAKE_MODE,
    )


@app.post("/chat", response_model=AiTextResponse)
def chat(request: ChatRequest):
    history_block = "\n".join([f"{message.role}: {message.content}" for message in request.history])
    prompt = (
        "You are MiniCoursera AI assistant. Be concise, supportive and educational."
        f"\nRespond in language code: {request.language}."
        "\nIf asked for harmful or disallowed content, refuse briefly."
        f"\nConversation history:\n{history_block}"
        f"\nUser message:\n{request.message}"
    )
    return _generate(prompt)


@app.post("/summary", response_model=AiTextResponse)
def summary(request: SummaryRequest):
    prompt = (
        "Summarize the following learning content for a student."
        f"\nLanguage: {request.language}."
        f"\nReturn exactly {request.max_bullets} concise bullet points."
        "\nAvoid markdown code blocks."
        f"\nContent:\n{request.text}"
    )
    return _generate(prompt)


@app.post("/quiz", response_model=AiTextResponse)
def quiz(request: QuizRequest):
    prompt = (
        "Create a multiple-choice quiz for the following learning content."
        f"\nLanguage: {request.language}."
        f"\nGenerate exactly {request.questions_count} questions."
        "\nEach question must include 4 options and one correct answer."
        "\nOutput as strict JSON array with fields: question, options, correctAnswer, explanation."
        f"\nContent:\n{request.text}"
    )
    return _generate(prompt)


if __name__ == "__main__":
    import uvicorn

    uvicorn.run("chatbot:app", host="0.0.0.0", port=8001, reload=True)
