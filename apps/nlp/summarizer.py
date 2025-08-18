import re
from typing import List

# Mock summarization function - in production, you'd use a real NLP model
def summarize_chunk(txt: str, max_len: int = 220, min_len: int = 130) -> str:
    """Summarize a text chunk with specified length constraints"""
    # Truncate input to reasonable size
    txt = txt[:4000]
    
    # Simple extractive summarization (in production, use HF pipeline)
    sentences = re.split(r'[.!?]+', txt)
    sentences = [s.strip() for s in sentences if s.strip()]
    
    # Score sentences by word count and position
    scored_sentences = []
    for i, sentence in enumerate(sentences):
        if len(sentence.split()) < 5:  # Skip very short sentences
            continue
        
        # Score based on position (earlier sentences get higher scores)
        position_score = 1.0 / (i + 1)
        length_score = min(len(sentence.split()) / 20.0, 1.0)  # Prefer medium-length sentences
        score = position_score * 0.7 + length_score * 0.3
        
        scored_sentences.append((score, sentence))
    
    # Sort by score and take top sentences
    scored_sentences.sort(key=lambda x: x[0], reverse=True)
    
    # Build summary within length constraints
    summary_parts = []
    current_length = 0
    
    for _, sentence in scored_sentences:
        sentence_length = len(sentence.split())
        if current_length + sentence_length <= max_len:
            summary_parts.append(sentence)
            current_length += sentence_length
        else:
            break
    
    summary = '. '.join(summary_parts) + '.'
    
    # Ensure minimum length
    if len(summary.split()) < min_len:
        # Add more sentences if needed
        for _, sentence in scored_sentences[len(summary_parts):]:
            summary_parts.append(sentence)
            if len(' '.join(summary_parts).split()) >= min_len:
                break
        summary = '. '.join(summary_parts) + '.'
    
    return summary

def reduce_summaries(text: str) -> str:
    """Generate final summary with target length 300-450 words"""
    # Split text into chunks
    chunks = split_into_chunks(text, max_chunk_size=2000)
    
    # Summarize each chunk
    chunk_summaries = []
    for chunk in chunks:
        summary = summarize_chunk(chunk, max_len=220, min_len=130)
        chunk_summaries.append(summary)
    
    # Combine chunk summaries
    combined = ' '.join(chunk_summaries)
    
    # Generate final summary
    final_summary = summarize_chunk(combined, max_len=520, min_len=320)
    
    return final_summary

def split_into_chunks(text: str, max_chunk_size: int = 2000) -> List[str]:
    """Split text into chunks of approximately equal size"""
    words = text.split()
    chunks = []
    
    current_chunk = []
    current_size = 0
    
    for word in words:
        if current_size + len(word) + 1 > max_chunk_size and current_chunk:
            chunks.append(' '.join(current_chunk))
            current_chunk = [word]
            current_size = len(word)
        else:
            current_chunk.append(word)
            current_size += len(word) + 1
    
    if current_chunk:
        chunks.append(' '.join(current_chunk))
    
    return chunks
