#!/usr/bin/env python3
"""
Test script to verify the executive summary improvements
"""

import requests
import json

def test_executive_summary():
    """Test the improved executive summary generation"""
    
    base_url = "http://localhost:8000"
    
    # Test with problematic text that was causing issues
    problematic_text = """(0123456789) Education and Information Technologies (2023) 28:5967–5997 https://doi.org/10.1007/s10639-023-12345-6 Keywords Machine learning · Artificial intelligence · K-12 · Systematic review 1 Introduction Popular interest in artificial intelligence (AI) has increased incredibly in recent times. Especially, machine learning (ML), an essential subset of AI that has become the new engine that revolutionizes practices of knowledge discovery (Lin et al, 2023). This paper presents a comprehensive analysis of machine learning applications in educational settings. We investigate the effectiveness of various AI-driven approaches in K-12 education and demonstrate significant improvements in student engagement and learning outcomes. Our research methodology combines quantitative analysis with qualitative assessment to provide a holistic understanding of the impact of artificial intelligence in educational technology."""
    
    # Test summarize endpoint with cleaned text
    summarize_data = {
        "paper_id": "test-exec-summary",
        "sections": [
            {
                "name": "abstract",
                "text": problematic_text,
                "tokens": len(problematic_text.split()),
                "pageStart": 1,
                "pageEnd": 1,
                "orderIdx": 0
            }
        ]
    }
    
    try:
        response = requests.post(f"{base_url}/summarize", json=summarize_data)
        print(f"Executive Summary Test: {response.status_code}")
        if response.status_code == 200:
            result = response.json()
            summary = result.get('summary', '')
            print(f"\n=== EXECUTIVE SUMMARY ===")
            print(f"Length: {len(summary)} characters")
            print(f"Content: {summary}")
            print(f"\n=== ANALYSIS ===")
            
            # Check for common issues
            issues = []
            if 'doi.org' in summary.lower():
                issues.append("❌ Contains DOI references")
            if 'http' in summary.lower():
                issues.append("❌ Contains URLs")
            if 'keywords' in summary.lower():
                issues.append("❌ Contains keyword sections")
            if 'abstract:' in summary.lower():
                issues.append("❌ Contains abstract headers")
            if summary.count('(') > summary.count(')'):
                issues.append("❌ Unbalanced parentheses")
            if not summary.endswith('.'):
                issues.append("❌ Doesn't end with period")
            if len(summary) < 50:
                issues.append("❌ Too short")
            if len(summary) > 1000:
                issues.append("❌ Too long")
            
            if not issues:
                print("✅ Executive summary looks good!")
            else:
                print("Issues found:")
                for issue in issues:
                    print(f"  {issue}")
                    
            print(f"\n=== OTHER SECTIONS ===")
            print(f"Abstract: {len(result.get('abstract', ''))} chars")
            print(f"Introduction: {len(result.get('introduction', ''))} chars")
            print(f"Methodology: {len(result.get('methodology', ''))} chars")
            print(f"Results: {len(result.get('results', ''))} chars")
            print(f"Discussion: {len(result.get('discussion', ''))} chars")
            print(f"Limitations: {len(result.get('limitations', ''))} chars")
            print(f"Technical Details: {len(result.get('technical_details', ''))} chars")
            print(f"Impact: {len(result.get('impact', ''))} chars")
            
        else:
            print(f"  Error: {response.text}")
    except Exception as e:
        print(f"Executive summary test failed: {e}")

if __name__ == "__main__":
    print("Testing Executive Summary Improvements...")
    test_executive_summary()
    print("\nTest completed!")
