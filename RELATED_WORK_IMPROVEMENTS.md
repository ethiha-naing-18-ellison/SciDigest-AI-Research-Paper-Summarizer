# Related Work Improvements: Direct Paper Links

## Overview

The related work functionality has been enhanced to provide direct links to academic papers instead of just Google Scholar search results. Users can now access papers directly from their original sources (arXiv, Semantic Scholar, etc.) with multiple link options.

## Key Improvements

### 1. Direct Paper Links
- **Before**: All related work links went to Google Scholar search results
- **After**: Primary links go directly to the paper's source (arXiv, Semantic Scholar, etc.)
- **Fallback**: Google Scholar is still available as an alternative option

### 2. Multiple Link Options
Each related paper now provides multiple ways to access it:
- **Primary Link**: Direct link to the paper (arXiv, Semantic Scholar, etc.)
- **Alternative URLs**: Additional sources for the same paper
- **Google Scholar**: As a fallback option

### 3. Enhanced UI
- **Link Buttons**: Multiple clickable buttons for each paper
- **Visual Indicators**: Primary links are highlighted in purple
- **Source Labels**: Clear indication of where each link will take you

## Technical Implementation

### Backend Changes

#### 1. Enhanced Related Work Generation (`apps/nlp/related_work.py`)
- Added `get_paper_urls()` function to generate direct links
- Updated curated papers with real arXiv URLs
- Added support for alternative URLs

#### 2. Academic Search Service (`apps/nlp/academic_search.py`)
- New service to search across multiple academic databases
- Support for arXiv, Semantic Scholar, and OpenAlex APIs
- Automatic deduplication of results

#### 3. Updated API Endpoint (`apps/nlp/main.py`)
- Modified `/related` endpoint to use enhanced related work generation
- Better error handling and fallback mechanisms

### Frontend Changes

#### 1. Enhanced RelatedGrid Component (`apps/web/src/components/RelatedGrid.tsx`)
- **Multiple Link Options**: Shows all available links for each paper
- **Primary Link Highlighting**: Direct paper links are visually distinguished
- **Better UX**: Users can choose their preferred source

#### 2. Updated Types (`apps/web/src/types/dto.ts`)
- Added `alternative_urls` field to `RelatedItem` interface
- Support for multiple URL options per paper

## Supported Academic Sources

### 1. arXiv
- **Direct Links**: `https://arxiv.org/abs/{paper_id}`
- **PDF Links**: `https://arxiv.org/pdf/{paper_id}`
- **Search**: `https://arxiv.org/search/?query={title}`

### 2. Semantic Scholar
- **Direct Links**: `https://www.semanticscholar.org/paper/{paper_id}`
- **Search**: `https://www.semanticscholar.org/search?q={title}`

### 3. OpenAlex
- **Direct Links**: `https://openalex.org/{work_id}`
- **Search**: `https://openalex.org/search?q={title}`

### 4. Google Scholar
- **Search**: `https://scholar.google.com/scholar?q={title}+{author}+{year}`

## Example Usage

When a user uploads a research paper, the system now:

1. **Analyzes the content** to identify relevant research areas
2. **Searches academic databases** for related papers
3. **Generates multiple links** for each related paper
4. **Presents options** to the user with clear source labels

### Sample Output
```
Related Paper: "Attention Is All You Need"
├── [arXiv] (Primary) - Direct link to paper
├── [PDF] - Direct PDF download
├── [Semantic Scholar] - Alternative source
└── [Google Scholar] - Search results
```

## Benefits

### For Users
- **Direct Access**: No need to search through Google Scholar
- **Multiple Sources**: Choose the most convenient source
- **Better Experience**: Faster access to papers
- **Source Transparency**: Know exactly where each link goes

### For Researchers
- **Efficient Workflow**: Direct links save time
- **Source Verification**: Access papers from their original publishers
- **Multiple Formats**: PDF, HTML, and search options available

## Future Enhancements

### Planned Improvements
1. **API Integration**: Better integration with academic APIs
2. **Citation Data**: Include citation counts and impact metrics
3. **Personalization**: Remember user's preferred sources
4. **Offline Access**: Cache frequently accessed papers

### Potential Sources
1. **PubMed**: For biomedical papers
2. **IEEE Xplore**: For engineering papers
3. **ACM Digital Library**: For computer science papers
4. **Springer Link**: For multidisciplinary papers

## Testing

Run the test script to verify functionality:
```bash
python test_related_work.py
```

This will test:
- Academic search service
- Related work generation
- URL generation and validation
- Multiple link options

## Configuration

The system can be configured via environment variables:
- `NEXT_PUBLIC_RELATED_LINK_MODE`: Controls link behavior
- API keys for academic databases (future enhancement)

## Conclusion

This enhancement significantly improves the user experience by providing direct access to academic papers while maintaining the flexibility of multiple source options. Users can now efficiently access related research without the overhead of search engines.
