import sys
sys.stdout.reconfigure(encoding="utf-8", errors="replace")

import rag_core

vs = rag_core.load_vector_store()

queries = [
    "Truoc khi hien mau nen an uong the nao?",
    "Nen an gi truoc khi hien mau?",
    "Co duoc uong ruou bia truoc khi hien mau khong?",
    "An uong",
    "ruou bia",
    "dau mo do an nhanh",
]

for q in queries:
    results = vs.similarity_search_with_score(q, k=3)
    print()
    print("Q:", q)
    for doc, score in results:
        src = doc.metadata.get("source", "?").split("\\")[-1]
        head = doc.page_content.strip().replace("\n", " ")[:55]
        print(f"   dist={score:.4f}  {src} | {head}")