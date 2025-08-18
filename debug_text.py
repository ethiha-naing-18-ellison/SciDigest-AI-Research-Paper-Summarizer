#!/usr/bin/env python3
"""
Debug script to see what's happening with text processing
"""

import re

def clean_pdf_text(text: str) -> str:
    """Clean and structure PDF text by removing artifacts and formatting issues"""
    if not text:
        return ""
    
    print(f"Original text length: {len(text)}")
    print(f"Original text preview: {text[:200]}...")
    
    # Remove common PDF artifacts
    import re
    
    # Remove page numbers and headers/footers
    text = re.sub(r'\b\d+\s*$', '', text, flags=re.MULTILINE)  # Page numbers at end of lines
    text = re.sub(r'^\d+\s*', '', text, flags=re.MULTILINE)    # Page numbers at start of lines
    
    # Remove DOI patterns
    text = re.sub(r'https?://doi\.org/[^\s]+', '', text)
    text = re.sub(r'doi:[^\s]+', '', text)
    
    # Remove email patterns
    text = re.sub(r'\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b', '', text)
    
    # Remove URL patterns
    text = re.sub(r'https?://[^\s]+', '', text)
    
    # Remove citation patterns like (Author et al, 2023)
    text = re.sub(r'\([^)]*et al[^)]*\)', '', text)
    text = re.sub(r'\([^)]*\d{4}[^)]*\)', '', text)
    
    # Remove keywords section
    text = re.sub(r'Keywords[:\s]*[^.]*\.', '', text, flags=re.IGNORECASE)
    text = re.sub(r'Key words[:\s]*[^.]*\.', '', text, flags=re.IGNORECASE)
    
    # Remove abstract/doi headers
    text = re.sub(r'Abstract[:\s]*', '', text, flags=re.IGNORECASE)
    
    # Clean up multiple spaces and newlines
    text = re.sub(r'\s+', ' ', text)
    text = re.sub(r'\n+', '\n', text)
    
    print(f"After basic cleaning: {len(text)}")
    print(f"After basic cleaning preview: {text[:200]}...")
    
    # Remove lines that are just numbers or special characters
    lines = text.split('\n')
    cleaned_lines = []
    for i, line in enumerate(lines):
        line = line.strip()
        # Skip lines that are mostly numbers, special chars, or too short
        if (len(line) > 5 and 
            not re.match(r'^[\d\s\-_\.]+$', line) and
            not re.match(r'^[A-Z\s]+$', line) and  # Skip all caps headers
            not line.startswith('(') and
            not line.endswith(')')):
            cleaned_lines.append(line)
        else:
            print(f"Removed line {i}: '{line}'")
    
    # Join lines and clean up
    result = ' '.join(cleaned_lines)
    result = re.sub(r'\s+', ' ', result).strip()
    
    print(f"Final result length: {len(result)}")
    print(f"Final result: {result}")
    
    return result

# Test with the problematic text
problematic_text = """(0123456789) Education and Information Technologies (2023) 28:5967–5997 https://doi.org/10.1007/s10639-023-12345-6 Keywords Machine learning · Artificial intelligence · K-12 · Systematic review 1 Introduction Popular interest in artificial intelligence (AI) has increased incredibly in recent times. Especially, machine learning (ML), an essential subset of AI that has become the new engine that revolutionizes practices of knowledge discovery (Lin et al, 2023). This paper presents a comprehensive analysis of machine learning applications in educational settings. We investigate the effectiveness of various AI-driven approaches in K-12 education and demonstrate significant improvements in student engagement and learning outcomes. Our research methodology combines quantitative analysis with qualitative assessment to provide a holistic understanding of the impact of artificial intelligence in educational technology."""

print("=== DEBUGGING TEXT CLEANING ===")
cleaned = clean_pdf_text(problematic_text)
print(f"\n=== FINAL RESULT ===")
print(f"Length: {len(cleaned)}")
print(f"Content: {cleaned}")
