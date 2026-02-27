import json
import os
import re
import sys
from typing import List, Optional

from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field, validator

load_dotenv()

try:
    from google import genai
except Exception:
    genai = None


# ============== Request Models with Enhanced Validation ==============

class ChatMessage(BaseModel):
    role: str = Field(description="user|assistant", min_length=1, max_length=20)
    content: str = Field(min_length=1, max_length=4000)

    @validator('role')
    def validate_role(cls, v):
        if v not in ['user', 'assistant', 'system']:
            return 'user'
        return v


class ChatRequest(BaseModel):
    message: str = Field(min_length=1, max_length=4000)
    history: Optional[List[ChatMessage]] = Field(default_factory=list)
    language: str = Field(default="en", min_length=2, max_length=8)
    context: Optional[str] = Field(default=None, max_length=10000)
    strict_grounded: bool = Field(default=False)

    @validator('message', always=True)
    def validate_message(cls, v):
        if not v or not v.strip():
            raise ValueError("Message cannot be empty or whitespace only")
        # Check for potentially malicious input patterns
        if len(v) > 4000:
            raise ValueError("Message exceeds maximum length of 4000 characters")
        return v.strip()


class SummaryRequest(BaseModel):
    text: str = Field(min_length=10, max_length=25000)
    max_bullets: int = Field(default=5, ge=3, le=15)
    language: str = Field(default="en", min_length=2, max_length=8)
    mode: str = Field(default="short", description="short|detailed")

    @validator('text', always=True)
    def validate_text(cls, v):
        if not v or not v.strip():
            raise ValueError("Text content cannot be empty")
        if len(v.strip()) < 10:
            raise ValueError("Text must be at least 10 characters")
        return v.strip()

    @validator('mode')
    def validate_mode(cls, v):
        if v not in ['short', 'detailed']:
            return 'short'
        return v


class QuizRequest(BaseModel):
    text: str = Field(min_length=30, max_length=25000)
    questions_count: int = Field(default=5, ge=3, le=15)
    language: str = Field(default="en", min_length=2, max_length=8)
    difficulty: str = Field(default="medium", description="easy|medium|hard")
    include_explanations: bool = Field(default=True)

    @validator('text', always=True)
    def validate_text(cls, v):
        if not v or not v.strip():
            raise ValueError("Text content cannot be empty")
        if len(v.strip()) < 30:
            raise ValueError("Text must be at least 30 characters for quiz generation")
        return v.strip()

    @validator('difficulty')
    def validate_difficulty(cls, v):
        if v not in ['easy', 'medium', 'hard']:
            return 'medium'
        return v


class AiTextResponse(BaseModel):
    output: str
    provider: str
    model: str
    safe: bool = True  # Indicates if response passed safety checks


class HealthResponse(BaseModel):
    status: str
    provider: str
    fake_mode: bool


# ============== Configuration ==============

API_KEY = os.getenv("GEMINI_API_KEY", "")
MODEL_NAME = os.getenv("GEMINI_MODEL", "gemini-2.5-flash")
FAKE_MODE = os.getenv("AI_FAKE_MODE", "false").lower() == "true"

# Log mode at startup
print(f"[AI Service] FAKE_MODE={FAKE_MODE}, GEMINI_API_KEY set={bool(API_KEY)}", file=sys.stderr)

app = FastAPI(
    title="MiniCoursera AI Service",
    version="2.0.0",
    description="AI microservice for chat, summaries, and quiz generation with enhanced features",
)


# ============== Helper Functions ==============

def _extract_text(response) -> Optional[str]:
    """Extract text from AI response with multiple fallback strategies."""
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


def _safe_content_check(text: str) -> bool:
    """
    Check if content is safe and appropriate.
    Returns False if content appears to be harmful or unsafe.
    """
    if not text:
        return False

    # Check for empty or very short responses
    if len(text.strip()) < 5:
        return False

    # Check for common error indicators in AI responses
    error_indicators = [
        "i'm sorry, but i can't help",
        "i cannot fulfill this request",
        "sorry, i can't help",
        "i'm unable to",
        "error",
        "exception"
    ]

    text_lower = text.lower()
    for indicator in error_indicators:
        if indicator in text_lower and len(text) < 200:
            return False

    return True


def _format_safe_response(output: str, provider: str, model: str) -> AiTextResponse:
    """Format response with safety check."""
    is_safe = _safe_content_check(output)
    
    # If response is not safe, return a fallback message
    if not is_safe:
        return AiTextResponse(
            output="I apologize, but I'm unable to provide a response to that request. Please try rephrasing your question or ask about the course content.",
            provider=provider,
            model=model,
            safe=False
        )
    
    return AiTextResponse(output=output, provider=provider, model=model, safe=True)


def _fake_response(prompt: str, endpoint: str) -> str:
    """Generate fake response for testing."""
    normalized = " ".join(prompt.split())
    snippet = normalized[:300]
    
    responses = {
        "chat": f"[FAKE_MODE CHAT RESPONSE]\nThis is a deterministic test response.\nYour prompt snippet: {snippet}",
        "summary": "[FAKE_MODE SUMMARY]\n• Key point 1 from content\n• Key point 2 from content\n• Key point 3 from content\n• Key point 4 from content\n• Key point 5 from content",
        "quiz": json.dumps([
            {
                "question": "Sample Question 1?",
                "options": ["Option A", "Option B", "Option C", "Option D"],
                "correctAnswer": "Option A",
                "explanation": "This is the explanation for the correct answer."
            },
            {
                "question": "Sample Question 2?",
                "options": ["Option A", "Option B", "Option C", "Option D"],
                "correctAnswer": "Option B",
                "explanation": "This is the explanation for the correct answer."
            }
        ])
    }
    
    return responses.get(endpoint, f"[FAKE_MODE] Endpoint: {endpoint}")


def _generate(prompt: str, endpoint: str = "chat") -> AiTextResponse:
    """Generate AI response with error handling and safety checks."""
    if FAKE_MODE:
        output = _fake_response(prompt, endpoint)
        return _format_safe_response(output, "fake", "local-test")

    if not API_KEY or genai is None:
        raise HTTPException(
            status_code=500,
            detail="Gemini provider not configured. Set GEMINI_API_KEY or enable AI_FAKE_MODE=true.",
        )

    try:
        client = genai.Client(api_key=API_KEY)
        
        # Configure safety settings
        safety_settings = [
            {"category": "HARM_CATEGORY_HARASSMENT", "threshold": "BLOCK_MEDIUM_AND_ABOVE"},
            {"category": "HARM_CATEGORY_HATE_SPEECH", "threshold": "BLOCK_MEDIUM_AND_ABOVE"},
            {"category": "HARM_CATEGORY_SEXUALLY_EXPLICIT", "threshold": "BLOCK_MEDIUM_AND_ABOVE"},
            {"category": "HARM_CATEGORY_DANGEROUS_CONTENT", "threshold": "BLOCK_MEDIUM_AND_ABOVE"},
        ]
        
        response = client.models.generate_content(
            model=MODEL_NAME, 
            contents=prompt,
            config={
                "safety_settings": safety_settings,
                "temperature": 0.7,
                "max_output_tokens": 4096,
            }
        )
        
        text = _extract_text(response)
        if not text:
            raise HTTPException(status_code=502, detail="No text returned by AI provider")
        
        return _format_safe_response(text, "gemini", MODEL_NAME)
        
    except HTTPException:
        raise
    except Exception as ex:
        raise HTTPException(status_code=502, detail=f"AI provider call failed: {str(ex)}")


# ============== Enhanced Prompt Engineering ==============

def _build_quiz_prompt(request: QuizRequest) -> str:
    """Build optimized quiz generation prompt with difficulty levels."""
    
    difficulty_instructions = {
        "easy": "Questions should be straightforward, testing basic recall and understanding. "
                "Use simple language and direct questions.",
        "medium": "Questions should test comprehension and application. "
                  "Require some analysis and understanding of concepts.",
        "hard": "Questions should be challenging, testing synthesis and evaluation. "
                "Include scenario-based questions, comparisons, and edge cases."
    }
    
    difficulty = request.difficulty.lower() if request.difficulty else "medium"
    difficulty_instruction = difficulty_instructions.get(difficulty, difficulty_instructions["medium"])
    
    explanation_instruction = "Include a brief explanation for why the answer is correct." if request.include_explanations else "Do not include explanations."
    
    # Map language codes to language names for better prompting
    language_map = {
        "en": "English", "fr": "French", "es": "Spanish", "de": "German",
        "ar": "Arabic", "zh": "Chinese", "ja": "Japanese", "pt": "Portuguese"
    }
    language_name = language_map.get(request.language.lower(), "English")
    
    prompt = f"""You are an expert educational content creator for an online learning platform.
Your task is to create a high-quality multiple-choice quiz based on the provided learning content.

## Requirements:
1. Generate exactly {request.questions_count} questions
2. Each question must have exactly 4 options (A, B, C, D)
3. Clearly indicate the correct answer
4. {explanation_instruction}
5. Questions should be relevant to the content provided

## Difficulty Level: {difficulty.upper()}
{difficulty_instruction}

## Output Format:
Return a valid JSON array with the following structure:
[
  {{
    "question": "Question text here?",
    "options": ["Option A", "Option B", "Option C", "Option D"],
    "correctAnswer": "Option A",
    "explanation": "Brief explanation here (if enabled)"
  }}
]

## Language:
Respond in {language_name}.

## Content to base quiz on:
{request.text}

Generate the quiz now:"""
    
    return prompt


def _build_summary_prompt(request: SummaryRequest) -> str:
    """Build optimized summary prompt with short/detailed modes."""
    
    mode = request.mode.lower() if request.mode else "short"
    
    # Map language codes to language names
    language_map = {
        "en": "English", "fr": "French", "es": "Spanish", "de": "German",
        "ar": "Arabic", "zh": "Chinese", "ja": "Japanese", "pt": "Portuguese"
    }
    language_name = language_map.get(request.language.lower(), "English")
    
    if mode == "detailed":
        mode_instruction = """Provide a detailed, comprehensive summary.
- Write in paragraph form (not just bullet points)
- Cover all major concepts thoroughly
- Include examples where appropriate
- Explain relationships between concepts
- Use {max_bullets} or more substantive points in the response""".format(max_bullets=request.max_bullets)
    else:
        mode_instruction = f"""Provide a concise summary with exactly {request.max_bullets} key bullet points.
- Each bullet should be brief and clear
- Focus on the most important concepts
- Use simple language
- Avoid unnecessary details"""
    
    prompt = f"""You are an expert tutor creating a summary for students.
Your task is to summarize the provided learning content.

## Summary Mode: {mode.upper()}
{mode_instruction}

## Language:
Respond in {language_name}.

## Formatting:
- Do not use markdown code blocks
- Use clear, student-friendly language
- Focus on learning objectives and key takeaways

## Content to summarize:
{request.text}

Generate the summary now:"""
    
    return prompt


def _build_chat_prompt(request: ChatRequest) -> str:
    """Build optimized chat prompt with context and grounded behavior."""
    
    # Map language codes to language names
    language_map = {
        "en": "English", "fr": "French", "es": "Spanish", "de": "German",
        "ar": "Arabic", "zh": "Chinese", "ja": "Japanese", "pt": "Portuguese"
    }
    language_name = language_map.get(request.language.lower(), "English")
    
    # Build system instructions
    system_instruction = """You are MiniCoursera AI assistant, a helpful educational tutor.
- Be concise, supportive, and encouraging
- Use simple language suitable for students
- If you don't know something, say so honestly
- Always prioritize accurate information from the provided context"""
    
    if request.strict_grounded:
        system_instruction += """
- STRICT MODE: Only answer based on the provided context
- If the answer cannot be derived from the context, explicitly state that
- Do not make up information or hallucinate"""
    
    # Build history context
    history_block = ""
    if request.history:
        formatted_history = []
        for msg in request.history[-12:]:  # Limit to last 12 messages
            role = "User" if msg.role == "user" else "Assistant"
            formatted_history.append(f"{role}: {msg.content}")
        history_block = "Conversation history:\n" + "\n".join(formatted_history) + "\n\n"
    
    # Build context section
    context_block = ""
    if request.context:
        context_block = f"""## Provided Context:
{request.context}

"""
    
    prompt = f"""{system_instruction}

## Language:
Respond in {language_name}.

{context_block}{history_block}## Current Question:
{request.message}

Provide your response:"""
    
    return prompt


# ============== API Endpoints ==============

@app.get("/health", response_model=HealthResponse)
def health_check():
    return HealthResponse(
        status="ok",
        provider="fake" if FAKE_MODE else "gemini",
        fake_mode=FAKE_MODE,
    )


@app.post("/chat", response_model=AiTextResponse)
def chat(request: ChatRequest):
    """Chat endpoint with contextual awareness and grounded behavior."""
    try:
        prompt = _build_chat_prompt(request)
        return _generate(prompt, "chat")
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Chat processing error: {str(e)}")


@app.post("/summary", response_model=AiTextResponse)
def summary(request: SummaryRequest):
    """Summary endpoint with short/detailed modes."""
    try:
        prompt = _build_summary_prompt(request)
        return _generate(prompt, "summary")
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Summary processing error: {str(e)}")


@app.post("/quiz", response_model=AiTextResponse)
def quiz(request: QuizRequest):
    """Quiz endpoint with difficulty levels and configurable questions."""
    try:
        prompt = _build_quiz_prompt(request)
        return _generate(prompt, "quiz")
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Quiz processing error: {str(e)}")


if __name__ == "__main__":
    import uvicorn
    uvicorn.run("chatbot:app", host="0.0.0.0", port=8001, reload=True)
