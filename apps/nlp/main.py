# apps/nlp/main.py
from fastapi import FastAPI
from fastapi.responses import RedirectResponse
from pydantic import BaseModel
from typing import List, Optional

app = FastAPI(title="SciDigest NLP", version="0.1.0")

# --- health & root ---
@app.get("/")
def root():
    return RedirectResponse("/docs")

@app.get("/healthz")
def healthz():
    return {"status": "ok"}

# --- schemas (stubs to keep API shape) ---
class Section(BaseModel):
    name: str
    text: str
    tokens: int
    pageStart: int
    pageEnd: int
    orderIdx: int

class ParseInput(BaseModel):
    paper_id: str
    file_path: str

class ParseMeta(BaseModel):
    title: Optional[str] = None
    authors: Optional[str] = None
    year: Optional[int] = None
    venue: Optional[str] = None

class ParseResult(BaseModel):
    meta: Optional[ParseMeta] = None
    sections: List[Section]

class SummarizeInput(BaseModel):
    paper_id: str
    sections: List[Section]

class Anchor(BaseModel):
    bulletIndex: int
    sectionName: str
    pageStart: int
    pageEnd: int

class SummarizeResult(BaseModel):
    summary: str
    contributions: List[str]
    details: List[str] | None = None       # NEW (optional)
    anchors: List[Anchor]

class RelatedInput(BaseModel):
    title: Optional[str] = None
    keyphrases: Optional[List[str]] = None
    sections: Optional[List[Section]] = None

class RelatedItem(BaseModel):
    title: str
    authors: str
    venue: Optional[str] = None
    year: Optional[int] = None
    url: Optional[str] = None
    reason: str

class RelatedPayload(BaseModel):
    provider: str
    items: List[RelatedItem]

# --- endpoints (stubbed) ---
@app.post("/parse", response_model=ParseResult)
def parse(inp: ParseInput):
    return {
        "meta": {"title": None, "authors": None, "year": None, "venue": None},
        "sections": [{
            "name": "abstract",
            "text": "Replace with real PyMuPDF parsing.",
            "tokens": 7,
            "pageStart": 1,
            "pageEnd": 1,
            "orderIdx": 0
        }]
    }

def make_detail(bullet: str, sections_text: list[str]) -> str:
    """
    Build a short, concrete explanation (2–3 sentences) using context from the
    most relevant section text. Fallback to the bullet itself if no context.
    """
    # pick the first section containing a substring match; otherwise join a bit of intro/results
    ctx = ""
    for txt in sections_text:
        if bullet[:30].lower() in txt.lower():
            ctx = txt
            break
    if not ctx and sections_text:
        ctx = " ".join(sections_text[:2])[:2000]

    # Very light reduction: pick 2–3 informative sentences from ctx.
    import re
    sents = [s.strip() for s in re.split(r'(?<=[.!?])\s+', ctx) if 40 <= len(s.strip()) <= 300]
    if not sents:
        return bullet
    # take up to 2–3 diverse sentences
    chosen = sents[:3]
    return " ".join(chosen[:3])

@app.post("/summarize", response_model=SummarizeResult)
def summarize(inp: SummarizeInput):
    bullets = [f"Key contribution #{i+1} (stub)" for i in range(8)]
    anchors = [{"bulletIndex": i, "sectionName": "abstract", "pageStart": 1, "pageEnd": 1} for i in range(len(bullets))]
    
    # Generate details for each bullet
    sections_text = [s.text for s in inp.sections]
    details = [make_detail(b, sections_text) for b in bullets]
    
    return {
        "summary": "Stub executive summary. Replace with real model output.",
        "contributions": bullets,
        "details": details,                     # NEW
        "anchors": anchors
    }

@app.post("/related", response_model=RelatedPayload)
def related(inp: RelatedInput):
    items = [{
        "title": f"Related paper {i+1}",
        "authors": "A. Author; B. Researcher",
        "venue": "DemoConf",
        "year": 2024,
        "url": "https://example.org",
        "reason": "Stub rationale."
    } for i in range(10)]
    return {"provider": "OpenAlex+SemanticScholar", "items": items}
