import re
from typing import List, Dict, Any, Tuple

def extract_contributions(sections: List[Dict[str, Any]]) -> Tuple[List[str], List[Dict[str, Any]]]:
    """Extract contributions (up to 10) with anchors"""
    
    # Cue verbs that indicate contributions
    cues = [
        "propose", "introduce", "present", "demonstrate", "outperform", 
        "achieve", "reduce", "improve", "show", "establish", "validate",
        "develop", "design", "implement", "evaluate", "analyze", "compare",
        "extend", "enhance", "optimize", "solve", "address", "overcome"
    ]
    
    # Combine all text
    all_text = "\n\n".join([section.get("text", "") for section in sections])
    
    # Find sentences with contribution cues
    sentences = re.split(r'[.!?]+', all_text)
    candidate_lines = []
    
    for sentence in sentences:
        sentence = sentence.strip()
        if not sentence or len(sentence.split()) < 5:
            continue
            
        # Check if sentence contains contribution cues
        sentence_lower = sentence.lower()
        for cue in cues:
            if cue in sentence_lower:
                # Clean up the sentence
                cleaned = re.sub(r'\s+', ' ', sentence).strip()
                if 50 <= len(cleaned) <= 280:  # Length constraint
                    candidate_lines.append(cleaned)
                break
    
    # Deduplicate by normalized text
    deduped_lines = dedupe_norm(candidate_lines)
    
    # Take up to 10 contributions
    contributions = deduped_lines[:10]
    
    # Generate anchors (simplified)
    anchors = []
    for i, contribution in enumerate(contributions):
        # Find which section contains this contribution
        for section in sections:
            if contribution.lower() in section.get("text", "").lower():
                anchors.append({
                    "bulletIndex": i,
                    "sectionName": section.get("name", ""),
                    "pageStart": section.get("pageStart", 1),
                    "pageEnd": section.get("pageEnd", 1)
                })
                break
    
    return contributions, anchors

def dedupe_norm(lines: List[str]) -> List[str]:
    """Deduplicate lines by normalized text"""
    seen = set()
    result = []
    
    for line in lines:
        # Normalize: lowercase, remove extra spaces, punctuation
        normalized = re.sub(r'[^\w\s]', '', line.lower())
        normalized = re.sub(r'\s+', ' ', normalized).strip()
        
        if normalized not in seen:
            seen.add(normalized)
            result.append(line)
    
    return result
