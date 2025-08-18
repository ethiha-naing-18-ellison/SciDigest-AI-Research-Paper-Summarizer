#!/usr/bin/env python3
"""
Debug script to see the actual NLP response
"""

import requests
import json

def debug_nlp_response():
    """Debug the NLP service response"""
    
    base_url = "http://localhost:8000"
    
    # Test with clean text
    test_text = """Education and Information Technologies 28:5967–5997 Especially, machine learning (ML), an essential subset of AI that has become the new engine that revolutionizes practices of knowledge discovery. This paper presents a comprehensive analysis of machine learning applications in educational settings. We investigate the effectiveness of various AI-driven approaches in K-12 education and demonstrate significant improvements in student engagement and learning outcomes. Our research methodology combines quantitative analysis with qualitative assessment to provide a holistic understanding of the impact of artificial intelligence in educational technology."""
    
    # Test summarize endpoint
    summarize_data = {
        "paper_id": "test-debug",
        "sections": [
            {
                "name": "abstract",
                "text": test_text,
                "tokens": len(test_text.split()),
                "pageStart": 1,
                "pageEnd": 1,
                "orderIdx": 0
            }
        ]
    }
    
    try:
        print("Sending request to NLP service...")
        response = requests.post(f"{base_url}/summarize", json=summarize_data)
        print(f"Response status: {response.status_code}")
        
        if response.status_code == 200:
            result = response.json()
            print(f"\n=== FULL RESPONSE ===")
            print(json.dumps(result, indent=2))
            
            print(f"\n=== SECTION LENGTHS ===")
            print(f"Summary: {len(result.get('summary', ''))} chars")
            print(f"Abstract: {len(result.get('abstract', ''))} chars")
            print(f"Introduction: {len(result.get('introduction', ''))} chars")
            print(f"Methodology: {len(result.get('methodology', ''))} chars")
            print(f"Results: {len(result.get('results', ''))} chars")
            print(f"Discussion: {len(result.get('discussion', ''))} chars")
            print(f"Limitations: {len(result.get('limitations', ''))} chars")
            print(f"Technical Details: {len(result.get('technical_details', ''))} chars")
            print(f"Impact: {len(result.get('impact', ''))} chars")
            
            print(f"\n=== SECTION CONTENTS ===")
            for section in ['abstract', 'introduction', 'methodology', 'results', 'discussion', 'limitations', 'technical_details', 'impact']:
                content = result.get(section, '')
                print(f"{section.upper()}: {content[:100]}{'...' if len(content) > 100 else ''}")
            
        else:
            print(f"Error: {response.text}")
    except Exception as e:
        print(f"Debug failed: {e}")

if __name__ == "__main__":
    print("=== DEBUGGING NLP RESPONSE ===")
    debug_nlp_response()
    print("\nDebug completed!")
