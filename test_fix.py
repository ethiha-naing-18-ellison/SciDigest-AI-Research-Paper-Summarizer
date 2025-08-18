#!/usr/bin/env python3
"""
Test script to verify the NLP service fixes
"""

import requests
import json

def test_nlp_service():
    """Test the NLP service endpoints"""
    
    base_url = "http://localhost:8000"
    
    # Test health endpoint
    try:
        response = requests.get(f"{base_url}/healthz")
        print(f"Health check: {response.status_code} - {response.json()}")
    except Exception as e:
        print(f"Health check failed: {e}")
        return
    
    # Test parse endpoint (mock data)
    parse_data = {
        "paper_id": "test-123",
        "file_path": "/path/to/test.pdf"
    }
    
    try:
        response = requests.post(f"{base_url}/parse", json=parse_data)
        print(f"Parse endpoint: {response.status_code}")
        if response.status_code == 200:
            result = response.json()
            print(f"  Meta: {result.get('meta')}")
            print(f"  Sections: {len(result.get('sections', []))}")
        else:
            print(f"  Error: {response.text}")
    except Exception as e:
        print(f"Parse test failed: {e}")
    
    # Test summarize endpoint
    summarize_data = {
        "paper_id": "test-123",
        "sections": [
            {
                "name": "abstract",
                "text": "This is a test paper about machine learning and artificial intelligence. It presents novel methods for data analysis.",
                "tokens": 20,
                "pageStart": 1,
                "pageEnd": 1,
                "orderIdx": 0
            }
        ]
    }
    
    try:
        response = requests.post(f"{base_url}/summarize", json=summarize_data)
        print(f"Summarize endpoint: {response.status_code}")
        if response.status_code == 200:
            result = response.json()
            print(f"  Summary length: {len(result.get('summary', ''))}")
            print(f"  Contributions: {len(result.get('contributions', []))}")
            print(f"  Details: {len(result.get('details', []))}")
        else:
            print(f"  Error: {response.text}")
    except Exception as e:
        print(f"Summarize test failed: {e}")
    
    # Test related endpoint
    related_data = {
        "title": "Machine Learning Research",
        "keyphrases": ["machine learning", "artificial intelligence"],
        "sections": [
            {
                "name": "abstract",
                "text": "This paper discusses machine learning approaches.",
                "tokens": 10,
                "pageStart": 1,
                "pageEnd": 1,
                "orderIdx": 0
            }
        ]
    }
    
    try:
        response = requests.post(f"{base_url}/related", json=related_data)
        print(f"Related endpoint: {response.status_code}")
        if response.status_code == 200:
            result = response.json()
            print(f"  Provider: {result.get('provider')}")
            print(f"  Items: {len(result.get('items', []))}")
        else:
            print(f"  Error: {response.text}")
    except Exception as e:
        print(f"Related test failed: {e}")

if __name__ == "__main__":
    print("Testing NLP Service Fixes...")
    test_nlp_service()
    print("\nTest completed!")
