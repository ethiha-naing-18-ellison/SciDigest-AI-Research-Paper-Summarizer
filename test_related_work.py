#!/usr/bin/env python3
"""
Test script for the new related work functionality with direct paper links
"""

import sys
import os

# Add the nlp directory to the path
sys.path.append(os.path.join(os.path.dirname(__file__), 'apps', 'nlp'))

def test_related_work():
    """Test the related work functionality"""
    try:
        from related_work import get_related_works
        
        print("Testing related work generation...")
        
        # Test with a sample title
        title = "Transformer models for natural language processing"
        keyphrases = ["transformer", "NLP", "attention"]
        sections = [
            {"text": "This paper introduces a new transformer architecture for NLP tasks."}
        ]
        
        # Get related works
        related_papers = get_related_works(title, keyphrases, sections, limit=5)
        
        print(f"\nFound {len(related_papers)} related papers:")
        print("=" * 80)
        
        for i, paper in enumerate(related_papers, 1):
            print(f"\n{i}. {paper['title']}")
            print(f"   Authors: {paper['authors']}")
            print(f"   Venue: {paper['venue']} ({paper['year']})")
            print(f"   Primary URL: {paper['url']}")
            if paper.get('alternative_urls'):
                print(f"   Alternative URLs: {len(paper['alternative_urls'])} available")
                for j, alt_url in enumerate(paper['alternative_urls'][:2], 1):
                    print(f"     {j}. {alt_url}")
            print(f"   Reason: {paper['reason']}")
        
        print("\n" + "=" * 80)
        print("✅ Related work test completed successfully!")
        
        return True
        
    except Exception as e:
        print(f"❌ Error testing related work: {e}")
        import traceback
        traceback.print_exc()
        return False

def test_academic_search():
    """Test the academic search service"""
    try:
        from academic_search import search_service
        
        print("\nTesting academic search service...")
        
        # Test arXiv search
        print("Testing arXiv search...")
        arxiv_results = search_service.search_arxiv("transformer", 3)
        print(f"Found {len(arxiv_results)} arXiv papers")
        
        # Test Semantic Scholar search
        print("Testing Semantic Scholar search...")
        semantic_results = search_service.search_semantic_scholar("BERT", 3)
        print(f"Found {len(semantic_results)} Semantic Scholar papers")
        
        # Test combined search
        print("Testing combined search...")
        combined_results = search_service.search_papers("machine learning", 5)
        print(f"Found {len(combined_results)} combined results")
        
        print("✅ Academic search test completed successfully!")
        return True
        
    except Exception as e:
        print(f"❌ Error testing academic search: {e}")
        import traceback
        traceback.print_exc()
        return False

if __name__ == "__main__":
    print("🧪 Testing Related Work with Direct Paper Links")
    print("=" * 60)
    
    # Test academic search first
    search_success = test_academic_search()
    
    # Test related work generation
    related_success = test_related_work()
    
    if search_success and related_success:
        print("\n🎉 All tests passed! The new related work functionality is working.")
    else:
        print("\n⚠️  Some tests failed. Check the output above for details.")
        sys.exit(1)
