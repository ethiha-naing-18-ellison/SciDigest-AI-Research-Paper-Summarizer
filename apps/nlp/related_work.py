import os
from typing import List, Dict, Any, Optional

def get_related_works(title: Optional[str], keyphrases: Optional[List[str]], 
                     sections: Optional[List[Dict[str, Any]]], limit: int = 10) -> List[Dict[str, Any]]:
    """Get related works from multiple providers with increased limit"""
    
    # Mock implementation - in production, you'd call OpenAlex and Semantic Scholar APIs
    # For now, return mock data to demonstrate the structure
    
    mock_items = [
        {
            "title": "Deep Learning for Natural Language Processing",
            "authors": "Smith, J., Johnson, A., Williams, B.",
            "venue": "ACL Conference",
            "year": 2023,
            "url": "https://example.com/paper1",
            "reason": "Similar methodology and domain"
        },
        {
            "title": "Transformer Models in Research Paper Analysis",
            "authors": "Brown, C., Davis, D., Miller, E.",
            "venue": "EMNLP Conference",
            "year": 2023,
            "url": "https://example.com/paper2",
            "reason": "Related technical approach"
        },
        {
            "title": "Automated Scientific Literature Review",
            "authors": "Wilson, F., Taylor, G., Anderson, H.",
            "venue": "AAAI Conference",
            "year": 2022,
            "url": "https://example.com/paper3",
            "reason": "Similar application domain"
        },
        {
            "title": "NLP Techniques for Academic Paper Summarization",
            "authors": "Thomas, I., Jackson, J., White, K.",
            "venue": "ICLR Conference",
            "year": 2023,
            "url": "https://example.com/paper4",
            "reason": "Directly related to paper summarization"
        },
        {
            "title": "Machine Learning in Research Paper Classification",
            "authors": "Harris, L., Martin, M., Garcia, N.",
            "venue": "KDD Conference",
            "year": 2022,
            "url": "https://example.com/paper5",
            "reason": "Similar classification task"
        },
        {
            "title": "Neural Networks for Document Understanding",
            "authors": "Rodriguez, O., Lee, P., Clark, Q.",
            "venue": "NeurIPS Conference",
            "year": 2023,
            "url": "https://example.com/paper6",
            "reason": "Related neural architecture"
        },
        {
            "title": "Attention Mechanisms in Text Processing",
            "authors": "Lewis, R., Walker, S., Hall, T.",
            "venue": "ICML Conference",
            "year": 2022,
            "url": "https://example.com/paper7",
            "reason": "Similar attention-based approach"
        },
        {
            "title": "Multi-Modal Learning for Scientific Documents",
            "authors": "Young, U., King, V., Wright, W.",
            "venue": "CVPR Conference",
            "year": 2023,
            "url": "https://example.com/paper8",
            "reason": "Related multi-modal techniques"
        },
        {
            "title": "Knowledge Graph Construction from Research Papers",
            "authors": "Lopez, X., Hill, Y., Scott, Z.",
            "venue": "WWW Conference",
            "year": 2022,
            "url": "https://example.com/paper9",
            "reason": "Similar knowledge extraction task"
        },
        {
            "title": "BERT-based Models for Academic Text Analysis",
            "authors": "Green, A., Baker, B., Adams, C.",
            "venue": "ACL Conference",
            "year": 2023,
            "url": "https://example.com/paper10",
            "reason": "Similar BERT-based approach"
        },
        {
            "title": "Semantic Similarity in Research Literature",
            "authors": "Nelson, D., Carter, E., Mitchell, F.",
            "venue": "EMNLP Conference",
            "year": 2022,
            "url": "https://example.com/paper11",
            "reason": "Related semantic analysis"
        },
        {
            "title": "Automated Citation Analysis and Recommendation",
            "authors": "Perez, G., Roberts, H., Turner, I.",
            "venue": "JCDL Conference",
            "year": 2023,
            "url": "https://example.com/paper12",
            "reason": "Similar citation-based approach"
        }
    ]
    
    # Return items up to the specified limit
    return mock_items[:limit]
