import os
from dotenv import load_dotenv
from google import genai
import json

# Load API key from .env
load_dotenv()
API_KEY = os.getenv("GEMINI_API_KEY")

if not API_KEY:
    raise ValueError("GEMINI_API_KEY not found in .env")

# Create a Client and chat session
client = genai.Client(api_key=API_KEY)

# Print available models to help pick a correct model name
print("Available models:")
try:
    for m in client.models.list():
        print("-", getattr(m, "name", str(m)))
except Exception:
    print("(could not list models)")

# Use a supported model name (change this if you prefer another model)
chat = client.chats.create(model="models/gemini-2.5-flash")

print("🤖 Gemini Chatbot (type 'exit' to quit)\n")

while True:
    try:
        user_input = input("You: ").strip()
        if user_input.lower() in ["exit", "quit"]:
            print("Goodbye 👋")
            break
        if not user_input:
            continue

        # Send message to Gemini using the client/chat API
        response = chat.send_message(user_input)

        # Try common fields for reply text, fall back to JSON print
        reply = None
        if hasattr(response, "text") and response.text:
            reply = response.text
        else:
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
                    for k, v in obj.items():
                        if k == "text" and isinstance(v, str):
                            return v
                        res = _find_text(v)
                        if res:
                            return res
                elif isinstance(obj, list):
                    for item in obj:
                        res = _find_text(item)
                        if res:
                            return res
                return None

            if data:
                reply = _find_text(data)

        if reply:
            print("Gemini:", reply)
            print("-" * 50)
        else:
            try:
                print("Raw response:", json.dumps(data or response.__dict__, default=str, indent=2))
            except Exception:
                print("Raw response:", response)

    except KeyboardInterrupt:
        print("\nExiting...")
        break
    except Exception as e:
        print(f"\n⚠️ Error: {e}\n")
