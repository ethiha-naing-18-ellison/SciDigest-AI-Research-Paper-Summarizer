# Research Paper Summarizer API

A .NET 8 Web API for uploading, processing, and summarizing research papers with background processing pipeline.

## Features

- **PDF Upload & Storage**: Accept PDF uploads with validation and disk storage
- **Background Processing**: Hangfire-powered pipeline for parsing, summarizing, and finding related work
- **NLP Integration**: Typed HttpClient for external NLP services with stub mode for development
- **Export Capabilities**: Export summaries as Markdown or PDF using PuppeteerSharp
- **Robust Architecture**: Entity Framework Core, AutoMapper, FluentValidation, Serilog logging
- **API Documentation**: Swagger/OpenAPI integration
- **Database**: SQL Server with EF Core migrations

## Prerequisites

- .NET 8 SDK
- SQL Server (local or Docker)
- Docker (optional, for containerized deployment)

## Quick Start

### 1. Local Development

```bash
# Clone and navigate to the API directory
cd apps/api

# Restore packages
dotnet restore

# Update connection string in appsettings.Development.json
# Default: Server=localhost,1433;Database=ResearchDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;

# Run database migrations
dotnet ef database update

# Start the application
dotnet run
```

The API will be available at:
- **Swagger UI**: http://localhost:5108/swagger
- **Hangfire Dashboard**: http://localhost:5108/hangfire
- **Health Check**: http://localhost:5108/healthz

### 2. Docker Compose (Recommended)

```bash
# Start API + SQL Server
cd apps/api
docker compose -f docker/docker-compose.api+db.yml up -d

# View logs
docker compose -f docker/docker-compose.api+db.yml logs -f

# Stop services
docker compose -f docker/docker-compose.api+db.yml down
```

## Configuration

### appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost,1433;Database=ResearchDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
  },
  "Storage": {
    "FilesRoot": "C:\\data\\files"
  },
  "Nlp": {
    "BaseUrl": "http://localhost:8000",
    "Stub": true
  },
  "Limits": {
    "MaxPdfMb": 50
  },
  "CORS": {
    "AllowedOrigins": [ "http://localhost:3000" ]
  }
}
```

### Environment Variables (Docker)

- `ConnectionStrings__Default`: Database connection string
- `Storage__FilesRoot`: File storage directory (e.g., `/data/files`)
- `Nlp__BaseUrl`: NLP service URL
- `Nlp__Stub`: Enable stub mode for NLP service (true/false)
- `Limits__MaxPdfMb`: Maximum PDF file size in MB

## API Endpoints

### Papers

- `POST /api/papers` - Upload PDF paper
- `GET /api/papers/{id}` - Get paper details with sections, summary, contributions, and related work
- `POST /api/papers/{id}/process` - Re-enqueue paper processing
- `GET /api/papers/{id}/summary` - Get executive summary
- `GET /api/papers/{id}/related` - Get related work
- `POST /api/papers/{id}/export?format=md|pdf` - Export as Markdown or PDF

### System

- `GET /healthz` - Health check

## Database Migrations

```bash
# Create new migration
dotnet ef migrations add MigrationName -o Infrastructure/Migrations

# Apply migrations
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## Example Usage

### Upload a Paper

```bash
curl -X POST "http://localhost:5108/api/papers" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@paper.pdf"
```

Response:
```json
{
  "paperId": "123e4567-e89b-12d3-a456-426614174000"
}
```

### Get Paper Details

```bash
curl "http://localhost:5108/api/papers/123e4567-e89b-12d3-a456-426614174000"
```

### Export as Markdown

```bash
curl -X POST "http://localhost:5108/api/papers/123e4567-e89b-12d3-a456-426614174000/export?format=md" \
  --output paper-summary.md
```

### Export as PDF

```bash
curl -X POST "http://localhost:5108/api/papers/123e4567-e89b-12d3-a456-426614174000/export?format=pdf" \
  --output paper-summary.pdf
```

## Processing Pipeline

1. **Parse**: Extract sections, text, and metadata from PDF
2. **Summarize**: Generate executive summary and key contributions
3. **Related Work**: Find related papers using NLP service

Each stage is tracked in the `ProcessingJobs` table with state transitions:
- `queued` → `running` → `done`/`failed`

## Data Model

- **Papers**: Main paper entity with metadata and status
- **PaperSections**: Extracted sections with text and page ranges
- **Summaries**: Executive summaries (versioned)
- **Contributions**: Key contributions with anchor references
- **RelatedWorks**: Related papers from external services (cached)
- **ProcessingJobs**: Background job state tracking

## Testing

```bash
# Run tests
dotnet test

# Run specific test
dotnet test --filter "UploadTests"
```

## Development Notes

- **Stub Mode**: Set `Nlp:Stub=true` in configuration to use mock NLP responses
- **File Storage**: Files are stored in `Storage:FilesRoot` directory
- **PDF Generation**: Uses PuppeteerSharp with headless Chrome
- **Background Jobs**: Hangfire processes papers asynchronously
- **Validation**: FluentValidation for request validation
- **Logging**: Serilog with console output
- **Error Handling**: Global middleware with consistent error responses

## Production Considerations

1. **Database**: Use Azure SQL Database or SQL Server cluster
2. **File Storage**: Consider Azure Blob Storage or AWS S3
3. **Scaling**: Use multiple Hangfire servers for processing
4. **Security**: Implement authentication and authorization
5. **Monitoring**: Add Application Insights or similar
6. **PDF Generation**: Consider dedicated service for Puppeteer

## Troubleshooting

### Common Issues

1. **Database Connection**: Ensure SQL Server is running and connection string is correct
2. **File Permissions**: Ensure API has write access to `Storage:FilesRoot`
3. **PDF Generation**: Chromium download may take time on first run
4. **Hangfire Dashboard**: Only available in Development environment

### Logs

Check application logs for detailed error information:
```bash
# Docker logs
docker compose -f docker/docker-compose.api+db.yml logs api

# Local development
# Logs are output to console
```
