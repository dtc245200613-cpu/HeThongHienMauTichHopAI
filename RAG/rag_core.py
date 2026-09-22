import os
import time
from pathlib import Path
from functools import lru_cache

from dotenv import load_dotenv

from langchain_chroma import Chroma

from langchain_huggingface import HuggingFaceEmbeddings

from google import genai


# ============================================================
# CẤU HÌNH
# ============================================================

BASE_DIR = Path(__file__).resolve().parent

ENV_FILE = BASE_DIR / ".env"

load_dotenv(ENV_FILE)


DATA_DIR = BASE_DIR / "data"

CHROMA_DIR = BASE_DIR / "chroma_db"


# ============================================================
# GEMINI
# ============================================================

GEMINI_API_KEY = os.getenv("GEMINI_API_KEY")

GEMINI_MODEL = os.getenv(
    "GEMINI_MODEL",
    "gemini-3.8-flash"
)

GEMINI_FALLBACK_MODEL = os.getenv(
    "GEMINI_FALLBACK_MODEL",
    "gemini-2.5-flash-lite"
)


# ============================================================
# EMBEDDING
# ============================================================

EMBEDDING_MODEL_NAME = (
    "sentence-transformers/"
    "paraphrase-multilingual-MiniLM-L12-v2"
)


# ============================================================
# EMBEDDING MODEL
# ============================================================

@lru_cache(maxsize=1)
def get_embeddings():

    print("Đang tải Embedding Model...")

    embeddings = HuggingFaceEmbeddings(

        model_name=EMBEDDING_MODEL_NAME,

        model_kwargs={
            "device": "cpu"
        },

        encode_kwargs={
            "normalize_embeddings": True
        }
    )

    print("Đã tải Embedding Model.")

    return embeddings


# ============================================================
# LOAD CHROMA
# ============================================================

@lru_cache(maxsize=1)
def load_vector_store():

    if not CHROMA_DIR.exists():

        raise FileNotFoundError(
            "Không tìm thấy Chroma Database.\n"
            "Hãy chạy ingest.py trước."
        )

    print("Đang mở Chroma Database...")

    embeddings = get_embeddings()

    vector_store = Chroma(

        persist_directory=str(
            CHROMA_DIR
        ),

        embedding_function=embeddings
    )

    print("Đã mở Chroma Database.")

    return vector_store


# ============================================================
# TÌM TÀI LIỆU
# ============================================================

def retrieve_documents(
    question,
    k=4
):

    vector_store = load_vector_store()

    documents = vector_store.similarity_search(
        question,
        k=k
    )

    print(
        f"Tìm thấy {len(documents)} tài liệu liên quan."
    )

    return documents


# ============================================================
# FORMAT CONTEXT
# ============================================================

def format_documents(documents):

    if not documents:

        return (
            "Không tìm thấy tài liệu liên quan."
        )

    formatted = []

    for index, doc in enumerate(
        documents,
        start=1
    ):

        source = doc.metadata.get(
            "source",
            "Không rõ nguồn"
        )

        page = doc.metadata.get(
            "page"
        )

        if page is not None:

            source_info = (
                f"{source}, "
                f"trang {page + 1}"
            )

        else:

            source_info = source

        content = doc.page_content.strip()

        formatted.append(

            f"[TÀI LIỆU {index}]\n"
            f"Nguồn: {source_info}\n"
            f"{content}"
        )

    return "\n\n".join(formatted)


# ============================================================
# GEMINI CLIENT
# ============================================================

@lru_cache(maxsize=1)
def get_gemini_client():

    if not GEMINI_API_KEY:

        raise ValueError(
            "Không tìm thấy GEMINI_API_KEY.\n"
            "Kiểm tra file RAG/.env"
        )

    print("Đang khởi tạo Gemini...")

    client = genai.Client(
        api_key=GEMINI_API_KEY
    )

    print("Đã kết nối Gemini.")

    return client


# ============================================================
# BUILD PROMPT
# ============================================================

def build_prompt(
    question,
    context
):

    return f"""
Bạn là trợ lý AI của hệ thống
quản lý hiến máu tình nguyện.

Hãy trả lời câu hỏi dựa trên các tài liệu
được cung cấp bên dưới.

QUY TẮC:

1. Trả lời bằng tiếng Việt.

2. Ưu tiên thông tin trong tài liệu.

3. Không tự bịa thông tin.

4. Nếu tài liệu không có thông tin phù hợp,
hãy trả lời:

"Tài liệu hiện có chưa đủ thông tin để trả lời."

5. Trả lời ngắn gọn, rõ ràng.

6. Nếu tài liệu có nguồn,
hãy ghi nguồn ở cuối câu trả lời.

7. Không đưa ra thông tin y tế quan trọng
nếu tài liệu không cung cấp.

========================
TÀI LIỆU
========================

{context}

========================
CÂU HỎI
========================

{question}

========================
TRẢ LỜI
========================
""".strip()


# ============================================================
# KIỂM TRA LỖI TẠM THỜI
# ============================================================

def is_temporary_gemini_error(error):

    error_text = str(error).upper()

    temporary_errors = [

        "503",
        "UNAVAILABLE",
        "429",
        "RESOURCE_EXHAUSTED",
        "TIMEOUT",
        "DEADLINE",
        "INTERNAL"
    ]

    return any(
        error_code in error_text
        for error_code in temporary_errors
    )


# ============================================================
# GỌI GEMINI VỚI RETRY + FALLBACK
# ============================================================

def call_gemini(
    client,
    prompt
):

    models = [

        GEMINI_MODEL,

        GEMINI_FALLBACK_MODEL
    ]

    # loại model trùng nhau
    models = list(
        dict.fromkeys(models)
    )

    last_error = None

    for model in models:

        print()
        print(
            f"Đang sử dụng model: {model}"
        )

        # mỗi model thử tối đa 3 lần
        for attempt in range(1, 4):

            try:

                print(
                    f"Đang gọi Gemini "
                    f"(lần {attempt}/3)..."
                )

                response = (
                    client.models.generate_content(

                        model=model,

                        contents=prompt
                    )
                )

                if not response:

                    raise Exception(
                        "Gemini không trả về response."
                    )

                answer = getattr(
                    response,
                    "text",
                    None
                )

                if answer:

                    print(
                        f"Gemini trả lời thành công "
                        f"bằng {model}."
                    )

                    return answer.strip()

                raise Exception(
                    "Gemini không trả về nội dung."
                )

            except Exception as e:

                last_error = e

                print()
                print(
                    f"Lỗi Gemini với model "
                    f"{model}:"
                )

                print(
                    str(e)
                )

                # Nếu lỗi tạm thời
                if is_temporary_gemini_error(e):

                    if attempt < 3:

                        wait_time = 2 ** (
                            attempt - 1
                        )

                        print(
                            f"Chờ {wait_time} giây "
                            f"rồi thử lại..."
                        )

                        time.sleep(
                            wait_time
                        )

                        continue

                    else:

                        print(
                            "Đã thử 3 lần."
                        )

                        print(
                            "Chuyển sang model "
                            "dự phòng..."
                        )

                        break

                # lỗi không phải lỗi tạm thời
                else:

                    print(
                        "Đây không phải lỗi "
                        "tạm thời."
                    )

                    break

    raise Exception(
        "Không thể kết nối Gemini.\n"
        f"Lỗi cuối: {last_error}"
    )


# ============================================================
# HÀM ASK GEMINI
# ============================================================

def ask_gemini(
    question,
    documents
):

    if not question or not question.strip():

        return "Vui lòng nhập câu hỏi."

    context = format_documents(
        documents
    )

    prompt = build_prompt(
        question,
        context
    )

    client = get_gemini_client()

    answer = call_gemini(
        client,
        prompt
    )

    return answer


# ============================================================
# HÀM RAG CHÍNH
# ============================================================

def ask(
    question,
    k=4
):

    if not question or not question.strip():

        return "Vui lòng nhập câu hỏi."

    try:

        # ============================
        # 1. TÌM TÀI LIỆU
        # ============================

        print()
        print(
            "Đang tìm tài liệu..."
        )

        documents = retrieve_documents(
            question,
            k=k
        )

        if not documents:

            return (
                "Tài liệu hiện có chưa đủ "
                "thông tin để trả lời."
            )

        # ============================
        # 2. GỌI GEMINI
        # ============================

        print(
            "Đang gửi context cho Gemini..."
        )

        answer = ask_gemini(
            question,
            documents
        )

        return answer

    except Exception as e:

        print()
        print(
            "======================================"
        )

        print(
            "LỖI RAG:"
        )

        print(
            str(e)
        )

        print(
            "======================================"
        )

        return (
            "Lỗi RAG: "
            + str(e)
        )


# ============================================================
# TEST RAG
# ============================================================

if __name__ == "__main__":

    print()

    print(
        "======================================"
    )

    print(
        "      RAG HIẾN MÁU + GEMINI"
    )

    print(
        "======================================"
    )

    print()

    print(
        f"Model chính: {GEMINI_MODEL}"
    )

    print(
        f"Model dự phòng: "
        f"{GEMINI_FALLBACK_MODEL}"
    )

    print()

    question = input(
        "Nhập câu hỏi: "
    ).strip()

    if not question:

        print(
            "Bạn chưa nhập câu hỏi."
        )

        raise SystemExit

    print()

    answer = ask(
        question,
        k=4
    )

    print()

    print(
        "======================================"
    )

    print(
        "              TRẢ LỜI"
    )

    print(
        "======================================"
    )

    print()

    print(answer)

    print()