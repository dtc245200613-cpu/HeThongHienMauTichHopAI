# Hệ Thống Hiến Máu Tích Hợp AI

Ứng dụng web quản lý hiến máu (ASP.NET Core MVC) tích hợp trợ lý AI hỏi đáp dựa trên kỹ thuật **RAG** (Retrieval-Augmented Generation) sử dụng Gemini + ChromaDB.

## Kiến trúc

```
┌─────────────────────────┐        HTTP          ┌──────────────────────┐
│  ASP.NET Core MVC       │  POST /ask  ───────▶ │  FastAPI (RAG API)   │
│  (net10.0, SQLite)      │  :8000               │  LangChain + Chroma  │
│  http://localhost:5xxx  │                      │  Gemini Generative   │
└─────────────────────────┘                      └──────────────────────┘
```

- **Web app** (`Controllers/`, `Views/`, `Models/`, `Services/`) — quản lý người dùng, chiến dịch, đăng ký hiến máu, thống kê.
- **RAG service** (`RAG/`) — FastAPI đọc tài liệu trong `RAG/data/`, tạo vector store bằng ChromaDB và gọi Gemini để trả lời câu hỏi.
- **`Services/AIService.cs`** — client gọi sang RAG API tại `http://127.0.0.1:8000/`.

## Công nghệ

| Thành phần | Công nghệ |
|---|---|
| Backend web | ASP.NET Core MVC (.NET 10) |
| ORM / DB | Entity Framework Core + SQLite (`hienmau.db`) |
| Xác thực | Cookie Authentication, Session, phân quyền Admin/Coordinator |
| RAG API | FastAPI + Uvicorn |
| Vector store | ChromaDB (LangChain) |
| Embedding | `sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2` |
| LLM | Google Gemini (`google-genai`) |

## Yêu cầu môi trường

- .NET SDK 10
- Python 3.10+
- Google Gemini API key

## Cách chạy

### 1. RAG API

```powershell
cd RAG
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install fastapi uvicorn python-dotenv langchain-chroma langchain-huggingface langchain-community langchain-text-splitters google-genai
```

Tạo file `.env` từ mẫu và điền API key thật:

```powershell
Copy-Item .env.example .env
# sửa GEMINI_API_KEY trong .env
```

Nạp dữ liệu vào vector store (chạy một lần, hoặc khi thay đổi tài liệu):

```powershell
python ingest.py
```

Khởi động API:

```powershell
uvicorn api:app --host 127.0.0.1 --port 8000
```

Kiểm tra: `GET http://127.0.0.1:8000/` → `{"status":"ok"}`.

### 2. Web app

```powershell
dotnet restore
dotnet ef database update
dotnet run
```

Mở trình duyệt tại địa chỉ hiển thị trong console (xem `Properties/launchSettings.json`).

## Tài khoản mẫu

Được tạo tự động khi chạy lần đầu (`Program.cs`):

| Vai trò | Email | Mật khẩu |
|---|---|---|
| Coordinator | `dpv@gmail.com` | `123456` |
| Admin | `admin@gmail.com` | `123456` |

> ⚠️ Đây là tài khoản demo cho môi trường phát triển. Cần đổi mật khẩu và băm (hash) trước khi triển khai thật.

## Cấu trúc thư mục

```
Controllers/        # Account, AIController, Campaign, DonationRegistration, ...
Data/               # ApplicationDbContext, DesignTimeDbContextFactory
Migrations/         # EF Core migrations
Models/             # User, Campaign, DonationRegistration
Services/           # AIService (gọi RAG API)
Views/              # Razor views
RAG/
  ├─ api.py         # FastAPI endpoint /ask
  ├─ rag_core.py    # Chroma + Gemini logic
  ├─ ingest.py      # Nạp tài liệu vào vector store
  ├─ data/          # Tài liệu nguồn (txt)
  └─ .env.example   # Mẫu cấu hình biến môi trường
```

## Bảo mật

Các file sau **không** được đưa lên Git (xem `.gitignore`):

- `RAG/.env` — chứa `GEMINI_API_KEY`
- `hienmau.db`, `RAG/chroma_db/` — dữ liệu sinh ra khi chạy
- `bin/`, `obj/`, `.venv/`, `__pycache__/`

Hãy dùng `RAG/.env.example` làm mẫu và tự tạo `.env` ở máy của bạn.
