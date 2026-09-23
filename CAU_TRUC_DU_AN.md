# Cấu trúc dự án — Hệ Thống Hiến Máu Tích Hợp AI

> Tài liệu mô tả chi tiết cấu trúc, chức năng từng thành phần và luồng hoạt động của hệ thống.
> Xem [`README.md`](README.md) để biết cách cài đặt và chạy.

---

## 1. Tổng quan kiến trúc

Hệ thống gồm **hai tiến trình độc lập** giao tiếp qua HTTP:

```
┌──────────────────────────────┐         HTTP POST /ask        ┌─────────────────────────────┐
│   WEB APP (ASP.NET Core MVC) │  ─────────────────────────▶   │   RAG API (FastAPI)         │
│                              │        :8000                  │                             │
│   • Giao diện người dùng     │                               │   • ChromaDB (vector store) │
│   • Xác thực & phân quyền    │  ◀─────────────────────────   │   • Embedding Model         │
│   • Nghiệp vụ hiến máu       │       JSON { success, answer }│   • Google Gemini (LLM)     │
│   • SQLite (hienmau.db)      │                               │                             │
│   :5121                      │                               │   :8000                     │
└──────────────────────────────┘                               └─────────────────────────────┘
```

| Tiến trình | Công nghệ | Cổng | Vai trò |
|---|---|
| **Web App** | ASP.NET Core MVC (.NET 10) | 5121 | Nghiệp vụ chính, giao diện, lưu trữ dữ liệu |
| **RAG API** | FastAPI + Uvicorn (Python 3.10+) | 8000 | Trả lời câu hỏi dựa trên tài liệu (RAG) |

---

## 2. Sơ đồ thư mục

```
HeThongHienMauTichHopAI/
│
├── Program.cs                       # Điểm khởi động: DI, DB, Auth, Session, seed dữ liệu
├── HeThongHienMauTichHopAI.csproj   # Cấu hình project .NET 10
├── appsettings.json                 # Cấu hình chung
├── appsettings.Development.json     # Cấu hình môi trường phát triển
├── hienmau.db                       # CSDL SQLite (sinh ra khi chạy, không commit)
│
├── Properties/
│   └── launchSettings.json          # Cấu hình chạy: profile http (5121), https (7036)
│
├── Controllers/                     # Tầng xử lý request
│   ├── HomeController.cs            # Trang chủ, Privacy, Error
│   ├── AccountController.cs         # Đăng nhập / Đăng ký / Đăng xuất / AccessDenied
│   ├── CampaignController.cs        # Danh sách & tạo chiến dịch hiến máu
│   ├── DonationRegistrationController.cs  # Form đăng ký hiến máu + xử lý
│   ├── RegistrationManagementController.cs# Duyệt / từ chối đăng ký (Admin, Coordinator)
│   ├── StatisticsController.cs      # Thống kê (Admin, Coordinator)
│   └── AIController.cs              # Giao diện chatbot + gọi RAG API
│
├── Models/                          # Thực thể dữ liệu
│   ├── User.cs                      # Người dùng hệ thống
│   ├── Campaign.cs                  # Chiến dịch hiến máu
│   ├── DonationRegistration.cs      # Phiếu đăng ký hiến máu
│   └── ErrorViewModel.cs            # Dữ liệu cho trang lỗi
│
├── Data/                            # Tầng truy cập dữ liệu
│   ├── ApplicationDbContext.cs      # DbContext: Users, Campaigns, DonationRegistrations
│   └── DesignTimeDbContextFactory.cs# Hỗ trợ `dotnet ef` lúc thiết kế
│
├── Migrations/                      # Lịch sử thay đổi schema (EF Core)
│   ├── 20260913021625_InitialCreate.cs
│   ├── 20260914130938_AddDonationRegistration.cs
│   └── 20260914135159_SyncDonationRegistration.cs
│
├── Services/                        # Tầng nghiệp vụ
│   └── AIService.cs                 # HTTP client gọi RAG API (127.0.0.1:8000)
│
├── Views/                           # Giao diện Razor
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   ├── Home/            Index.cshtml, Privacy.cshtml
│   ├── Account/         Login.cshtml, Register.cshtml, AccessDenied.cshtml
│   ├── Campaign/        Index.cshtml, Create.cshtml
│   ├── DonationRegistration/  Register.cshtml, Success.cshtml, Index.cshtml
│   ├── RegistrationManagement/ Index.cshtml
│   ├── Statistics/      Index.cshtml
│   ├── AI/              Index.cshtml
│   ├── Shared/          _Layout.cshtml, Error.cshtml, _ValidationScriptsPartial.cshtml
│   └── Create.cshtml                # ⚠️ View mồ côi ở gốc (xem mục 7)
│
├── wwwroot/                         # Tài nguyên tĩnh
│   ├── css/site.css
│   ├── js/site.js
│   └── lib/                         # Bootstrap, jQuery
│
└── RAG/                             # Dịch vụ RAG (Python)
    ├── api.py                       # FastAPI: GET / , POST /ask
    ├── rag_core.py                  # Chroma + Embedding + Gemini + retry/fallback
    ├── ingest.py                    # Nạp tài liệu vào ChromaDB
    ├── test_gemini.py               # Script kiểm tra kết nối Gemini
    ├── _inspect_chunks.py           # Script phụ: xem các đoạn đã chia
    ├── .env                         # API key & model (KHÔNG commit)
    ├── .env.example                 # Mẫu cấu hình
    ├── data/                        # Tài liệu nguồn (.txt)
    │   ├── huong_dan_hien_mau.txt
    │   └── chuan_bi_hien_mau_chi_tiet.txt
    └── chroma_db/                   # Vector store (sinh ra khi ingest, KHÔNG commit)
```

---

## 3. Mô hình dữ liệu

### 3.1 `User` — Người dùng

| Trường | Kiểu | Ràng buộc | Ghi chú |
|---|---|
| `Id` | int | Khóa chính | |
| `FullName` | string | `[Required]` | Họ tên hiển thị |
| `Email` | string | `[Required]`, `[EmailAddress]` | Dùng làm tên đăng nhập |
| `Password` | string | `[Required]` | ⚠️ Lưu **dạng thô**, chưa băm |
| `Role` | string | Mặc định `"Volunteer"` | `Volunteer` / `Coordinator` / `Admin` |

**Vai trò (Role):**

| Role | Quyền |
|---|---|
| `Volunteer` | Đăng ký hiến máu, xem chiến dịch, hỏi chatbot |
| `Coordinator` | + Duyệt đăng ký, xem thống kê |
| `Admin` | + Toàn quyền như Coordinator |

### 3.2 `Campaign` — Chiến dịch hiến máu

| Trường | Kiểu | Ghi chú |
|---|---|---|
| `Id` | int | Khóa chính |
| `Name` | string | Tên chiến dịch |
| `Location` | string | Địa điểm tổ chức |
| `Date` | DateTime | Ngày diễn ra |
| `Description` | string | Mô tả |
| `MaxParticipants` | int | Số người tối đa (dùng để chặn đăng ký quá số lượng) |

### 3.3 `DonationRegistration` — Phiếu đăng ký hiến máu

| Trường | Kiểu | Ràng buộc | Ghi chú |
|---|---|
| `Id` | int | Khóa chính | |
| `VolunteerName` | string | `[Required]` | Họ tên người hiến |
| `BloodType` | string | `[Required]` | `A+ A- B+ B- AB+ AB- O+ O-` |
| `CampaignId` | int | `[Required]` | Tham chiếu `Campaign.Id` |
| `Status` | string | Mặc định `"Chờ duyệt"` | `Chờ duyệt` / `Đã duyệt` / `Từ chối` |
| `RegisteredAt` | DateTime | Mặc định `DateTime.Now` | Thời điểm đăng ký |

> **Lưu ý:** `Campaign` và `DonationRegistration` chưa khai báo khóa ngoại (navigation property) — liên kết hiện được xử lý thủ công bằng truy vấn `CampaignId`.

---

## 4. Tầng Web App (ASP.NET Core MVC)

### 4.1 `Program.cs` — Cấu hình khởi động

| Bước | Nội dung |
|---|---|
| 1 | `AddControllersWithViews()` — bật MVC |
| 2 | `AddDbContext<ApplicationDbContext>` — SQLite, file `hienmau.db` |
| 3 | `AddHttpClient<AIService>()` + `AddScoped<AIService>()` — đăng ký dịch vụ AI |
| 4 | `AddSession()` — session 2 giờ |
| 5 | `AddAuthentication().AddCookie()` — cookie auth, hết hạn 2 giờ, login path `/Account/Login` |
| 6 | **Seed dữ liệu**: tạo 2 tài khoản mẫu nếu chưa có |
| 7 | Middleware: `ExceptionHandler` → `HttpsRedirection` → `StaticFiles` → `Routing` → `Session` → `Authentication` → `Authorization` |
| 8 | Route mặc định: `{controller=Home}/{action=Index}/{id?}` |

**Tài khoản seed tự động:**

| Vai trò | Email | Mật khẩu |
|---|---|---|
| Coordinator | `dpv@gmail.com` | `123456` |
| Admin | `admin@gmail.com` | `123456` |

### 4.2 Bảng định tuyến (Routes)

| URL | Controller/Action | Quyền | Mô tả |
|---|---|
| `/` | `Home/Index` | Công khai | Trang chủ |
| `/Home/Privacy` | `Home/Privacy` | Công khai | Trang chính sách |
| `/Account/Login` | `Account/Login` | Công khai | Đăng nhập |
| `/Account/Register` | `Account/Register` | Công khai | Đăng ký tài khoản |
| `/Account/Logout` | `Account/Logout` | Đã đăng nhập | Đăng xuất (POST) |
| `/Account/AccessDenied` | `Account/AccessDenied` | Công khai | Báo không có quyền |
| `/Campaign` | `Campaign/Index` | Công khai | Danh sách chiến dịch |
| `/Campaign/Create` | `Campaign/Create` | Công khai | Tạo chiến dịch |
| `/DonationRegistration` | `DonationRegistration/Index` | Công khai | Danh sách đăng ký |
| `/DonationRegistration/Register` | `DonationRegistration/Register` | Công khai | Form đăng ký hiến máu |
| `/DonationRegistration/Success` | `DonationRegistration/Success` | Công khai | Xác nhận thành công |
| `/RegistrationManagement` | `RegistrationManagement/Index` | **Coordinator, Admin** | Quản lý đăng ký |
| `/RegistrationManagement/Approve` | `.../Approve` | **Coordinator, Admin** | Duyệt (POST) |
| `/RegistrationManagement/Reject` | `.../Reject` | **Coordinator, Admin** | Từ chối (POST) |
| `/Statistics` | `Statistics/Index` | **Coordinator, Admin** | Thống kê |
| `/AI` | `AI/Index` | Công khai | Giao diện chatbot |
| `/AI/Ask` | `AI/Ask` | Công khai | Gửi câu hỏi (POST) |

### 4.3 Controller chi tiết

#### `AccountController` — Xác thực

| Action | Method | Xử lý |
|---|---|---|
| `Login` | GET | Hiện form đăng nhập |
| `Login` | POST | So khớp `Email` + `Password` trực tiếp trong DB → tạo cookie với 4 claim (`NameIdentifier`, `Name`, `Email`, `Role`) |
| `Register` | GET | Hiện form đăng ký |
| `Register` | POST | Kiểm tra trùng email, khớp mật khẩu xác nhận → tạo user role `Volunteer` |
| `Logout` | POST | `SignOutAsync` → về trang chủ |
| `AccessDenied` | GET | Trang báo không đủ quyền |

#### `DonationRegistrationController` — Nghiệp vụ chính

`POST Register` thực hiện kiểm tra theo thứ tự:

1. `ModelState.IsValid` — dữ liệu form hợp lệ
2. **Chiến dịch tồn tại?** → nếu không, báo *"Chiến dịch không tồn tại."*
3. **Còn chỗ không?** — đếm số đăng ký hiện có, so với `MaxParticipants`; nếu đạt trần, báo *"Chiến dịch đã đủ số lượng đăng ký."*
4. Gán `Status = "Chờ duyệt"`, `RegisteredAt = DateTime.Now` → lưu → chuyển tới `Success`

> ⚠️ **Điểm cần lưu ý:** `Campaign.MaxParticipants` mặc định là `0` nếu không nhập khi tạo chiến dịch. Khi đó điều kiện `totalRegistrations >= 0` luôn đúng → **mọi đăng ký đều bị chặn**. Cần nhập số người tối đa > 0 khi tạo chiến dịch.

#### `CampaignController`

- `Index` — liệt kê chiến dịch, sắp xếp giảm dần theo `Date`
- `Create` (GET) — form tạo mới
- `Create` (POST) — có `[ValidateAntiForgeryToken]`, lưu và quay về `Index`

#### `RegistrationManagementController` & `StatisticsController`

- Cả hai gắn `[Authorize(Roles = "Coordinator,Admin")]`
- `Approve(id)` → `Status = "Đã duyệt"`; `Reject(id)` → `Status = "Từ chối"`
- `Statistics.Index` đếm qua `ViewBag`: tổng, chờ duyệt, đã duyệt, từ chối, và số lượng theo từng nhóm máu (`APlus`, `AMinus`, `BPlus`, `BMinus`, `ABPlus`, `ABMinus`, `OPlus`, `OMinus`)

### 4.4 `AIService` — Cầu nối sang RAG

| Thuộc tính | Giá trị |
|---|---|
| `BaseAddress` | `http://127.0.0.1:8000/` |
| `Timeout` | 5 phút (đủ cho embedding + Gemini) |
| Endpoint gọi | `POST ask` với body `{ "question": "..." }` |

**Xử lý lỗi trả về thông báo thân thiện thay vì crash:**

| Tình huống | Thông báo |
|---|---|
| Câu hỏi rỗng | *"Vui lòng nhập câu hỏi."* |
| Không kết nối được RAG | *"Không thể kết nối RAG Python... http://127.0.0.1:8000"* |
| Quá thời gian | *"RAG xử lý quá lâu..."* |
| HTTP lỗi | *"Lỗi kết nối RAG: HTTP {mã}"* |

`RagResponse` là DTO nội bộ giải mã `{ success: bool, answer: string }`.

---

## 5. Tầng RAG (Python)

### 5.1 `ingest.py` — Nạp dữ liệu (chạy một lần)

```
data/*.txt  →  DirectoryLoader  →  RecursiveCharacterTextSplitter  →  Chroma.from_documents
                                        (chunk 500, overlap 50)              ↓
                                                                        chroma_db/
```

- Đọc mọi file `.txt` trong `data/` bằng UTF-8
- Chia nhỏ: mỗi đoạn 500 ký tự, chồng lấn 50 ký tự
- **Xóa collection cũ trước khi nạp** để tránh trùng lặp dữ liệu

### 5.2 `rag_core.py` — Lõi RAG

**Cấu hình (đọc từ `.env`):**

| Biến | Mặc định | Ghi chú |
|---|---|---|
| `GEMINI_API_KEY` | — | Bắt buộc |
| `GEMINI_MODEL` | `gemini-3.5-flash-lite` | Model chính |
| `GEMINI_FALLBACK_MODEL` | `gemini-3.5-flash` | Model dự phòng |
| `EMBEDDING_MODEL_NAME` | `sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2` | Chạy CPU, chuẩn hóa vector |

**Luồng xử lý `ask(question, k=4)`:**

```
1. retrieve_documents()  → Chroma similarity_search, lấy 4 đoạn gần nhất
2. format_documents()    → gắn số thứ tự + nguồn tài liệu
3. build_prompt()        → Prompt tiếng Việt kèm 7 quy tắc chống bịa thông tin
4. call_gemini()         → Gọi Gemini
5. Trả về câu trả lời
```

**Cơ chế chịu lỗi trong `call_gemini()`:**

| Loại lỗi | Hành vi |
|---|---|
| Hết quota (`RESOURCE_EXHAUSTED`, `QUOTA`) | Bỏ retry, **chuyển ngay sang model dự phòng** |
| Lỗi tạm thời (503, 429, TIMEOUT, INTERNAL) | Thử lại tối đa **3 lần**, chờ lũy tiến 1s → 2s → 4s |
| Lỗi khác (401 sai key, 404 sai model) | Dừng, chuyển model dự phòng |

Nếu **cả hai model đều thất bại**: ném ngoại lệ với thông báo rõ ràng (phân biệt hết quota vs lỗi kết nối).

**Bộ nhớ đệm:** `@lru_cache(maxsize=1)` cho `get_embeddings()`, `load_vector_store()`, `get_gemini_client()` — chỉ khởi tạo một lần cho cả tiến trình.

### 5.3 `api.py` — REST API

| Endpoint | Method | Request | Response |
|---|---|
| `/` | GET | — | `{"status":"ok","message":"RAG API đang hoạt động"}` |
| `/ask` | POST | `{"question":"..."}` | `{"success":true,"answer":"..."}` |

- Ép `stdout`/`stderr` sang **UTF-8** ngay đầu file để tránh lỗi in tiếng Việt trên Windows (console mặc định cp1252)
- `k=4` — lấy 4 đoạn tài liệu liên quan nhất
- Mọi ngoại lệ được bắt và trả về `{"success":false,"answer":"Lỗi RAG: ..."}` thay vì HTTP 500

---

## 6. Luồng hoạt động

### 6.1 Luồng nghiệp vụ hiến máu

```
Người dùng                    Web App                        SQLite
    │                            │                             │
    │  GET /Campaign             │                             │
    │───────────────────────────▶│  SELECT Campaigns ─────────▶│
    │◀──── danh sách ───────────│◀────────────────────────────│
    │                            │                             │
    │  GET /DonationRegistration/Register                   │
    │───────────────────────────▶│  load danh sách chiến dịch  │
    │◀──── form ─────────────────│                             │
    │                            │                             │
    │  POST /Register (form data + anti-forgery token)        │
    │───────────────────────────▶│                             │
    │                            │  1. Validate ModelState     │
    │                            │  2. Chiến dịch tồn tại?     │
    │                            │  3. Còn chỗ trống?          │
    │                            │  4. INSERT ────────────────▶│
    │◀── redirect /Success ──────│                             │
```

### 6.2 Luồng đăng nhập & phân quyền

```
POST /Account/Login  →  truy vấn User(email, password)
                     →  tạo ClaimsIdentity: Name, Email, Role
                     →  SignInAsync → cookie
                     →  redirect /

Request sau  →  Cookie Auth Middleware  →  ClaimsPrincipal
             →  [Authorize(Roles="...")] kiểm tra role
             →  thiếu quyền? → redirect /Account/AccessDenied
```

### 6.3 Luồng hỏi đáp AI

```
Người dùng            Web App (AIService)          RAG API              Chroma / Gemini
    │                       │                            │                      │
    │ POST /AI/Ask          │                            │                      │
    │ {question}            │                            │                      │
    │──────────────────────▶│  POST /ask                 │                      │
    │                       │───────────────────────────▶│                      │
    │                       │                            │  similarity_search ─▶│
    │                       │                            │◀── 4 đoạn tài liệu ──│
    │                       │                            │                      │
    │                       │                            │  build prompt + ────▶│
    │                       │                            │  gọi Gemini          │
    │                       │                            │◀─── câu trả lời ─────│
    │                       │◀── {success, answer} ──────│                      │
    │◀── hiển thị (Razor) ──│                            │                      │
```

---

## 7. Điểm cần lưu ý / Nợ kỹ thuật

| # | Vấn đề | Mức độ | Đề xuất |
|---|---|
| 1 | **Mật khẩu lưu dạng thô** — `User.Password` so khớp trực tiếp | 🔴 Cao | Dùng `BCrypt` hoặc `PasswordHasher<T>` của ASP.NET |
| 2 | **`MaxParticipants` mặc định 0** chặn mọi đăng ký nếu không nhập | 🔴 Cao | Thêm validation `[Range(1, int.MaxValue)]` cho `Campaign` |
| 3 | **Trang quản trị không được bảo vệ** — `/Campaign/Create`, `/DonationRegistration` để công khai | 🟠 Trung bình | Thêm `[Authorize(Roles="Admin,Coordinator")]` |
| 4 | **`Views/Create.cshtml` ở gốc** — view mồ côi, không controller nào dùng | 🟡 Thấp | Xóa hoặc chuyển vào thư mục đúng |
| 5 | **Thiếu khóa ngoại** giữa `Campaign` và `DonationRegistration` | 🟡 Thấp | Thêm navigation property + `OnDelete` |
| 6 | **Không có unit test** cho bất kỳ tầng nào | 🟠 Trung bình | Thêm project xUnit cho Controller/Service |
| 7 | **API key trong `.env.example`** (đã sửa thành placeholder) | 🔴 Cao | Thu hồi key cũ nếu đã từng commit |
| 8 | **Quota Gemini free tier** chỉ 20 request/ngày | 🟡 Thấp | Nâng gói hoặc thêm cache câu trả lời |

---

## 8. Cấu hình

### 8.1 `appsettings.json` / `appsettings.Development.json`
Cấu hình logging và chuỗi kết nối (hiện chuỗi SQLite khai báo trực tiếp trong `Program.cs`).

### 8.2 `Properties/launchSettings.json`

| Profile | URL |
|---|---|
| `http` | `http://localhost:5121` |
| `https` | `https://localhost:7036` + `http://localhost:5121` |

### 8.3 `RAG/.env`

```ini
GEMINI_API_KEY=<api_key_của_bạn>
GEMINI_MODEL=gemini-3.5-flash-lite
GEMINI_FALLBACK_MODEL=gemini-3.5-flash
```

### 8.4 `.gitignore` — Không commit

`RAG/.env` · `hienmau.db*` · `RAG/chroma_db/` · `bin/` · `obj/` · `.venv/` · `__pycache__/`

---

## 9. Tham chiếu nhanh

### Chạy hệ thống

```powershell
# 1. RAG API
cd RAG
.\.venv\Scripts\Activate.ps1
python ingest.py                          # chỉ lần đầu / khi đổi tài liệu
uvicorn api:app --host 127.0.0.1 --port 8000

# 2. Web App (cửa sổ khác)
dotnet restore
dotnet ef database update
dotnet run
```

### Kiểm tra nhanh

| Kiểm tra | Lệnh / URL |
|---|---|
| RAG API sống | `GET http://127.0.0.1:8000/` → `{"status":"ok"}` |
| Gemini hoạt động | `cd RAG; python test_gemini.py` |
| Web App sống | `http://localhost:5121` |
| Chatbot | `http://localhost:5121/AI` |

### Dừng hệ thống

```powershell
Get-Process HeThongHienMauTichHopAI,python -ErrorAction SilentlyContinue | Stop-Process -Force
```
