# SciDigest NLP Service

The NLP (Natural Language Processing) service for SciDigest AI Research Paper Summarizer. This service handles PDF parsing, summarization, contribution extraction, and related work discovery.

## Features

- **PDF Parsing**: Extract text and metadata from PDF files using PyMuPDF
- **Metadata Extraction**: Automatically detect title, authors, year, and venue
- **Longer Summaries**: Generate comprehensive executive summaries (300-450 words)
- **Enhanced Contributions**: Extract up to 10 key contributions with anchors
- **Related Work Discovery**: Find up to 10-12 related papers from multiple sources

## Setup

### Prerequisites

- Python 3.8+
- pip

### Installation

1. Navigate to the NLP service directory:
   ```bash
   cd apps/nlp
   ```

2. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```

3. Set environment variables (optional):
   ```bash
   # Create .env file
   echo "RELATED_LIMIT=10" > .env
   ```

## Running the Service

### Development

```bash
# Start the development server
uvicorn main:app --host 0.0.0.0 --port 8000 --reload
```

### Production

```bash
# Start the production server
uvicorn main:app --host 0.0.0.0 --port 8000
```

## API Endpoints

### POST /parse
Parse a PDF file and extract sections with metadata.

**Request:**
```json
{
  "paperId": "123e4567-e89b-12d3-a456-426614174000",
  "filePath": "/path/to/paper.pdf"
}
```

**Response:**
```json
{
  "meta": {
    "title": "Research Paper Title",
    "authors": "Author 1, Author 2",
    "year": 2023,
    "venue": "Conference Name"
  },
  "sections": [
    {
      "name": "Abstract",
      "text": "Abstract content...",
      "tokens": 150,
      "pageStart": 1,
      "pageEnd": 1,
      "orderIdx": 0
    }
  ]
}
```

### POST /summarize
Generate executive summary and extract contributions.

**Request:**
```json
{
  "paperId": "123e4567-e89b-12d3-a456-426614174000",
  "sections": [...]
}
```

**Response:**
```json
{
  "summary": "Comprehensive executive summary...",
  "contributions": [
    "First contribution",
    "Second contribution"
  ],
  "anchors": [
    {
      "bulletIndex": 0,
      "sectionName": "Introduction",
      "pageStart": 1,
      "pageEnd": 1
    }
  ]
}
```

### POST /related
Find related papers.

**Request:**
```json
{
  "title": "Paper Title",
  "keyphrases": ["keyword1", "keyword2"],
  "sections": [...]
}
```

**Response:**
```json
{
  "provider": "OpenAlex+SemanticScholar",
  "items": [
    {
      "title": "Related Paper Title",
      "authors": "Author 1, Author 2",
      "venue": "Conference Name",
      "year": 2023,
      "url": "https://example.com/paper",
      "reason": "Similar methodology"
    }
  ]
}
```

### GET /health
Health check endpoint.

**Response:**
```json
{
  "status": "healthy",
  "service": "nlp"
}
```

## Configuration

### Environment Variables

- `RELATED_LIMIT`: Maximum number of related papers to return (default: 10)

### Dependencies

- **FastAPI**: Web framework
- **PyMuPDF**: PDF processing
- **Pydantic**: Data validation
- **Uvicorn**: ASGI server

## Architecture

The NLP service is built with:

- **FastAPI**: Modern, fast web framework for building APIs
- **PyMuPDF**: High-performance PDF processing library
- **Modular Design**: Separate modules for different NLP tasks
- **Type Safety**: Full type hints and Pydantic models
- **Error Handling**: Comprehensive error handling and logging

## Development

### Project Structure

```
apps/nlp/
├── main.py              # FastAPI application
├── utils_meta.py        # Metadata extraction utilities
├── summarizer.py        # Summarization logic
├── contributions.py     # Contribution extraction
├── related_work.py      # Related work discovery
├── requirements.txt     # Python dependencies
├── .env                 # Environment variables
└── README.md           # This file
```

### Adding New Features

1. Create a new module for the feature
2. Add the endpoint to `main.py`
3. Update the API documentation
4. Add tests if applicable

## Integration

The NLP service is designed to work with the main SciDigest API. The API calls this service for:

- PDF parsing and metadata extraction
- Executive summary generation
- Contribution extraction
- Related work discovery

## Troubleshooting

### Common Issues

1. **PDF Processing Errors**: Ensure PyMuPDF is properly installed
2. **Memory Issues**: Large PDFs may require more memory
3. **Network Issues**: Check connectivity for related work discovery

### Logs

The service logs to stdout/stderr. Check the logs for detailed error information.

## License

This project is part of SciDigest AI Research Paper Summarizer.
