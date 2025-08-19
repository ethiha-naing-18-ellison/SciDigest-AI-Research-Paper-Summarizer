import requests
import time
from typing import List, Dict, Any, Optional
import json

class AcademicSearchService:
    """Service to search for academic papers across multiple databases"""
    
    def __init__(self):
        self.session = requests.Session()
        self.session.headers.update({
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
        })
    
    def search_arxiv(self, query: str, max_results: int = 5) -> List[Dict[str, Any]]:
        """Search arXiv for papers"""
        try:
            # arXiv API endpoint
            url = "http://export.arxiv.org/api/query"
            params = {
                'search_query': f'all:"{query}"',
                'start': 0,
                'max_results': max_results,
                'sortBy': 'relevance',
                'sortOrder': 'descending'
            }
            
            response = self.session.get(url, params=params, timeout=10)
            response.raise_for_status()
            
            # Parse XML response (simplified)
            papers = []
            content = response.text
            
            # Extract paper information (simplified parsing)
            import re
            titles = re.findall(r'<title>(.*?)</title>', content)
            authors = re.findall(r'<name>(.*?)</name>', content)
            summaries = re.findall(r'<summary>(.*?)</summary>', content)
            ids = re.findall(r'<id>(.*?)</id>', content)
            
            for i in range(min(len(titles), max_results)):
                if i == 0:  # Skip the first one (query title)
                    continue
                    
                paper_id = ids[i] if i < len(ids) else ""
                arxiv_id = paper_id.split('/')[-1] if paper_id else ""
                
                papers.append({
                    'title': titles[i] if i < len(titles) else f"Paper {i}",
                    'authors': authors[i] if i < len(authors) else "Unknown Authors",
                    'venue': 'arXiv',
                    'year': 2023,  # arXiv doesn't always provide year in search
                    'url': f"https://arxiv.org/abs/{arxiv_id}" if arxiv_id else "",
                    'reason': f"Related to {query}",
                    'alternative_urls': [
                        f"https://arxiv.org/pdf/{arxiv_id}" if arxiv_id else "",
                        f"https://www.semanticscholar.org/search?q={titles[i].replace(' ', '%20')}" if i < len(titles) else ""
                    ]
                })
            
            return papers
            
        except Exception as e:
            print(f"Error searching arXiv: {e}")
            return []
    
    def search_semantic_scholar(self, query: str, max_results: int = 5) -> List[Dict[str, Any]]:
        """Search Semantic Scholar for papers"""
        try:
            # Semantic Scholar API
            url = "https://api.semanticscholar.org/graph/v1/paper/search"
            params = {
                'query': query,
                'limit': max_results,
                'fields': 'title,authors.name,year,venue,url,abstract'
            }
            
            response = self.session.get(url, params=params, timeout=10)
            response.raise_for_status()
            
            data = response.json()
            papers = []
            
            for paper in data.get('data', []):
                authors = [author.get('name', '') for author in paper.get('authors', [])]
                authors_str = ', '.join(authors[:3]) + (' et al.' if len(authors) > 3 else '')
                
                papers.append({
                    'title': paper.get('title', 'Unknown Title'),
                    'authors': authors_str,
                    'venue': paper.get('venue', 'Unknown Venue'),
                    'year': paper.get('year', 2023),
                    'url': paper.get('url', ''),
                    'reason': f"Related to {query}",
                    'alternative_urls': [
                        f"https://www.semanticscholar.org/paper/{paper.get('paperId', '')}" if paper.get('paperId') else "",
                        f"https://scholar.google.com/scholar?q={paper.get('title', '').replace(' ', '+')}" if paper.get('title') else ""
                    ]
                })
            
            return papers
            
        except Exception as e:
            print(f"Error searching Semantic Scholar: {e}")
            return []
    
    def search_openalex(self, query: str, max_results: int = 5) -> List[Dict[str, Any]]:
        """Search OpenAlex for papers"""
        try:
            # OpenAlex API
            url = "https://api.openalex.org/works"
            params = {
                'search': query,
                'per_page': max_results,
                'select': 'title,authorships,publication_year,primary_location,abstract'
            }
            
            response = self.session.get(url, params=params, timeout=10)
            response.raise_for_status()
            
            data = response.json()
            papers = []
            
            for work in data.get('results', []):
                authors = [author.get('author', {}).get('display_name', '') for author in work.get('authorships', [])]
                authors_str = ', '.join(authors[:3]) + (' et al.' if len(authors) > 3 else '')
                
                papers.append({
                    'title': work.get('title', 'Unknown Title'),
                    'authors': authors_str,
                    'venue': work.get('primary_location', {}).get('source', {}).get('display_name', 'Unknown Venue'),
                    'year': work.get('publication_year', 2023),
                    'url': work.get('primary_location', {}).get('pdf_url', ''),
                    'reason': f"Related to {query}",
                    'alternative_urls': [
                        f"https://openalex.org/{work.get('id', '')}" if work.get('id') else "",
                        f"https://scholar.google.com/scholar?q={work.get('title', '').replace(' ', '+')}" if work.get('title') else ""
                    ]
                })
            
            return papers
            
        except Exception as e:
            print(f"Error searching OpenAlex: {e}")
            return []
    
    def search_papers(self, query: str, max_results: int = 8) -> List[Dict[str, Any]]:
        """Search across multiple academic databases"""
        all_papers = []
        
        # Search arXiv
        arxiv_papers = self.search_arxiv(query, max_results // 3)
        all_papers.extend(arxiv_papers)
        
        # Search Semantic Scholar
        semantic_papers = self.search_semantic_scholar(query, max_results // 3)
        all_papers.extend(semantic_papers)
        
        # Search OpenAlex
        openalex_papers = self.search_openalex(query, max_results // 3)
        all_papers.extend(openalex_papers)
        
        # Remove duplicates based on title similarity
        unique_papers = []
        seen_titles = set()
        
        for paper in all_papers:
            title_lower = paper['title'].lower()
            if title_lower not in seen_titles:
                seen_titles.add(title_lower)
                unique_papers.append(paper)
        
        return unique_papers[:max_results]

# Global instance
search_service = AcademicSearchService()
