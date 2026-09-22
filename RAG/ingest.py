from pathlib import Path

from langchain_community.document_loaders import DirectoryLoader, TextLoader
from langchain_huggingface import HuggingFaceEmbeddings
from langchain_chroma import Chroma
from langchain_text_splitters import RecursiveCharacterTextSplitter


# ===============================
# ĐƯỜNG DẪN
# ===============================

BASE_DIR = Path(__file__).resolve().parent

DATA_DIR = BASE_DIR / "data"
CHROMA_DIR = BASE_DIR / "chroma_db"


# ===============================
# MODEL EMBEDDING
# ===============================

EMBEDDING_MODEL = "sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2"


# ===============================
# ĐỌC TÀI LIỆU
# ===============================

print("===================================")
print("     RAG - TẠO CƠ SỞ DỮ LIỆU")
print("===================================")

print()
print("📂 Thư mục dữ liệu:")
print(DATA_DIR)

if not DATA_DIR.exists():
    print("❌ Không tìm thấy thư mục data.")
    exit()

print()
print("📖 Đang đọc tài liệu...")

loader = DirectoryLoader(
    str(DATA_DIR),
    glob="**/*.txt",
    loader_cls=TextLoader,
    loader_kwargs={
        "encoding": "utf-8"
    }
)

documents = loader.load()

print(f"✅ Đã đọc {len(documents)} tài liệu.")


# ===============================
# CHIA NHỎ TÀI LIỆU
# ===============================

print()
print("✂️ Đang chia nhỏ tài liệu...")

text_splitter = RecursiveCharacterTextSplitter(
    chunk_size=500,
    chunk_overlap=50
)

chunks = text_splitter.split_documents(documents)

print(f"✅ Đã tạo {len(chunks)} đoạn dữ liệu.")


# ===============================
# TẠO EMBEDDING
# ===============================

print()
print("🧠 Đang tải model embedding...")

embeddings = HuggingFaceEmbeddings(
    model_name=EMBEDDING_MODEL
)

print("✅ Embedding model đã sẵn sàng.")


# ===============================
# TẠO CHROMA DATABASE
# ===============================

print()
print("💾 Đang tạo ChromaDB...")

vectorstore = Chroma.from_documents(
    documents=chunks,
    embedding=embeddings,
    persist_directory=str(CHROMA_DIR)
)

print()
print("===================================")
print("✅ TẠO CHROMADB THÀNH CÔNG")
print("===================================")

print()
print(f"📂 ChromaDB:")
print(CHROMA_DIR)

print()
print("🎉 Hoàn tất!")