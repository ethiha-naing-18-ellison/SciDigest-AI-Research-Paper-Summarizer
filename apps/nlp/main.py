from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Optional, Dict, Any
import fitz  # PyMuPDF
import re
import os
from utils_meta import guess_title_and_authors
from summarizer import summarize_chunk, reduce_summaries
from related_work import get_related_works
from contributions import extract_contributions

app = FastAPI(title="SciDigest NLP Service", version="1.0.0")

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Models
class ParseInput(BaseModel):
    paperId: str
    filePath: str

class ParseMetaDto(BaseModel):
    title: Optional[str] = None
    authors: Optional[str] = None
    year: Optional[int] = None
    venue: Optional[str] = None

class SectionDto(BaseModel):
    name: str
    text: str
    tokens: int
    pageStart: int
    pageEnd: int
    orderIdx: int

class ParseResultDto(BaseModel):
    meta: Optional[ParseMetaDto] = None
    sections: List[SectionDto]

class SummarizeInput(BaseModel):
    paperId: str
    sections: List[SectionDto]

class SummarizeResponse(BaseModel):
    summary: str
    contributions: List[str]
    anchors: List[Dict[str, Any]]

class RelatedInput(BaseModel):
    title: Optional[str] = None
    keyphrases: Optional[List[str]] = None
    sections: Optional[List[SectionDto]] = None

class RelatedItem(BaseModel):
    title: str
    authors: str
    venue: Optional[str] = None
    year: Optional[int] = None
    url: Optional[str] = None
    reason: str

class RelatedResponse(BaseModel):
    provider: str
    items: List[RelatedItem]

@app.post("/parse", response_model=ParseResultDto)
async def parse_paper(input: ParseInput):
    """Parse PDF and extract sections with metadata"""
    try:
        # Open PDF with PyMuPDF
        doc = fitz.open(input.filePath)
        
        # Extract text from all pages
        pages_text = []
        for i in range(len(doc)):
            page = doc.load_page(i)
            pages_text.append(page.get_text("text"))
        
        # Extract metadata
        title, authors, year, venue = guess_title_and_authors(pages_text, doc.metadata or {})
        
        # Create metadata DTO
        meta = ParseMetaDto(
            title=title,
            authors=authors,
            year=year,
            venue=venue
        )
        
        # Extract sections (simplified for now)
        sections = []
        order_idx = 0
        
        for i, page_text in enumerate(pages_text):
            if page_text.strip():
                # Simple section extraction - in practice, you'd use more sophisticated NLP
                lines = page_text.split('\n')
                section_name = f"Page {i+1}"
                
                # Try to find section headers
                for line in lines[:10]:  # Check first 10 lines
                    line = line.strip()
                    if line and len(line) < 100 and line.isupper():
                        section_name = line
                        break
                
                sections.append(SectionDto(
                    name=section_name,
                    text=page_text,
                    tokens=len(page_text.split()),
                    pageStart=i+1,
                    pageEnd=i+1,
                    orderIdx=order_idx
                ))
                order_idx += 1
        
        doc.close()
        
        return ParseResultDto(meta=meta, sections=sections)
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to parse PDF: {str(e)}")

@app.post("/summarize", response_model=SummarizeResponse)
async def summarize_paper(input: SummarizeInput):
    """Generate executive summary and extract contributions"""
    try:
        # Combine all section text
        all_text = "\n\n".join([section.text for section in input.sections])
        
        # Generate longer summary (300-450 words)
        summary = reduce_summaries(all_text)
        
        # Extract contributions (up to 10)
        contributions, anchors = extract_contributions(input.sections)
        
        return SummarizeResponse(
            summary=summary,
            contributions=contributions,
            anchors=anchors
        )
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to summarize: {str(e)}")

@app.post("/related", response_model=RelatedResponse)
async def get_related(input: RelatedInput):
    """Get related papers (up to 10-12 items)"""
    try:
        # Get related works with increased limit
        limit = int(os.getenv("RELATED_LIMIT", "10"))
        items = get_related_works(input.title, input.keyphrases, input.sections, limit)
        
        return RelatedResponse(
            provider="OpenAlex+SemanticScholar",
            items=items
        )
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to get related works: {str(e)}")

@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy", "service": "nlp"}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
