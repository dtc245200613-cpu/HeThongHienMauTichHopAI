import sys

# Windows console mac dinh la cp1252 -> print tieng Viet se loi.
# Ep stdout/stderr sang UTF-8 truoc khi import rag_core.
if sys.stdout is not None:
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if sys.stderr is not None:
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")

from fastapi import FastAPI
from pydantic import BaseModel
import uvicorn

from rag_core import ask


app = FastAPI(
    title="Hệ thống RAG hiến máu",
    version="1.0"
)


class QuestionRequest(BaseModel):
    question: str


@app.get("/")
def home():
    return {
        "status": "ok",
        "message": "RAG API đang hoạt động"
    }


@app.post("/ask")
def ask_question(request: QuestionRequest):

    try:
        answer = ask(
            request.question,
            k=4
        )

        return {
            "success": True,
            "answer": answer
        }

    except Exception as e:

        return {
            "success": False,
            "answer": f"Lỗi RAG: {str(e)}"
        }


if __name__ == "__main__":

    uvicorn.run(
        app,
        host="127.0.0.1",
        port=8000,
        reload=False
    )