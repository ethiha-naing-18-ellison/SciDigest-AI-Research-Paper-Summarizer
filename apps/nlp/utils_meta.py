import re
from typing import Optional, Tuple

def guess_title_and_authors(pages_text: list[str], meta: dict) -> Tuple[Optional[str], Optional[str], Optional[int], Optional[str]]:
    """Extract title, authors, year, and venue from PDF pages and metadata"""
    title = (meta.get("title") or "").strip() or None
    authors = (meta.get("author") or "").strip() or None
    year = None
    venue = None

    first = (pages_text[0] if pages_text else "") or ""
    lines = [ln.strip() for ln in first.splitlines() if ln.strip()]

    def is_prob_title(s: str) -> bool:
        if len(s) < 5 or len(s) > 200: 
            return False
        low = s.lower()
        if "abstract" in low or "introduction" in low: 
            return False
        return True

    # Try to extract title if not in metadata
    if not title:
        for ln in lines[:6]:
            if is_prob_title(ln):
                title = ln
                break

    # Try to extract authors if not in metadata
    if not authors:
        # pick next lines with commas/and or 2–5 tokens/capitalized words
        for ln in lines[:8]:
            if ln == title: 
                continue
            if re.search(r"\b(and|,)\b", ln) or len(ln.split()) <= 10:
                if re.search(r"[A-Z][a-z]+", ln):
                    authors = ln
                    break

    # Guess year from first 2 pages
    scanned = "\n".join(pages_text[:2]) if pages_text else ""
    m = re.search(r"\b(19|20)\d{2}\b", scanned)
    if m:
        year = int(m.group(0))

    # Try to extract venue (conference/journal name)
    venue_patterns = [
        r"(?:Proceedings of |Conference on |Journal of |Transactions on )([A-Z][A-Za-z\s&]+)",
        r"([A-Z][A-Za-z\s&]+(?:Conference|Symposium|Workshop|Journal|Transactions))",
    ]
    
    for pattern in venue_patterns:
        matches = re.findall(pattern, scanned)
        if matches:
            venue = matches[0].strip()
            break

    return title, authors, year, venue
