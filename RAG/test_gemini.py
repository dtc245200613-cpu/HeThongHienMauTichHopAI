import os
import sys
# Windows console mặc định là cp1252 -> print tiếng Việt sẽ lỗi.
# Ép stdout/stderr sang UTF-8 trước khi in.
if sys.stdout is not None:
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if sys.stderr is not None:
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")

from dotenv import load_dotenv
from google import genai


# Load .env
load_dotenv()


API_KEY = os.getenv(
    "GEMINI_API_KEY"
)


MODEL = os.getenv(
    "GEMINI_MODEL",
    "gemini-2.5-flash"
)


print()
print("================================")
print("       TEST GEMINI API")
print("================================")
print()


if not API_KEY:

    print(
        "LỖI: Không tìm thấy GEMINI_API_KEY"
    )

    exit()


print(
    "Đã tìm thấy API KEY."
)

print(
    f"Model: {MODEL}"
)

print()


try:

    client = genai.Client(
        api_key=API_KEY
    )


    print(
        "Đang gửi câu hỏi đến Gemini..."
    )


    response = client.models.generate_content(

        model=MODEL,

        contents=(
            "Hãy trả lời bằng tiếng Việt: "
            "Hiến máu tình nguyện là gì?"
        )
    )


    print()
    print(
        "GEMINI TRẢ LỜI:"
    )

    print(
        response.text
    )

    print()
    print(
        "TEST THÀNH CÔNG."
    )


except Exception as e:

    print()
    print(
        "TEST THẤT BẠI."
    )

    print(
        str(e)
    )