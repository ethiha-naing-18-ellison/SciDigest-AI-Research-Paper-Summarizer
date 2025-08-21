using Api.Models;
using Api.Services;
using Api.Services.Export;
using Api.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PapersController : ControllerBase
{
    private readonly IPaperService _paperService;
    private readonly IExporter _exporter;
    private readonly ILogger<PapersController> _logger;

    public PapersController(
        IPaperService paperService,
        IExporter exporter,
        ILogger<PapersController> logger)
    {
        _paperService = paperService;
        _exporter = exporter;
        _logger = logger;
    }

    /// <summary>
    /// Upload a PDF paper for processing
    /// </summary>
    /// <param name="file">PDF file to upload</param>
    /// <returns>Paper ID</returns>
    [HttpPost]
    [ProducesResponseType<UploadResponse>(200)]
    [ProducesResponseType<ErrorResponse>(400)]
    public async Task<ActionResult<UploadResponse>> UploadPaper(IFormFile file)
    {
        try
        {
            var request = new UploadRequest { File = file };
            
            // Validation is handled by FluentValidation middleware
            var paperId = await _paperService.UploadAsync(file);
            
            return Ok(new UploadResponse { PaperId = paperId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload paper");
            return BadRequest(new ErrorResponse
            {
                Error = "UploadFailed",
                Message = "Failed to upload paper",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Re-enqueue paper processing
    /// </summary>
    /// <param name="id">Paper ID</param>
    [HttpPost("{id}/process")]
    [ProducesResponseType(200)]
    [ProducesResponseType<ErrorResponse>(404)]
    public async Task<IActionResult> ProcessPaper(Guid id)
    {
        try
        {
            var paper = await _paperService.GetPaperAsync(id);
            if (paper == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "NotFound",
                    Message = "Paper not found",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            await _paperService.EnqueueProcessingAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue processing for paper {PaperId}", id);
            return BadRequest(new ErrorResponse
            {
                Error = "ProcessingFailed",
                Message = "Failed to enqueue processing",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Get paper details with summary, contributions, and related work
    /// </summary>
    /// <param name="id">Paper ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType<PaperResponse>(200)]
    [ProducesResponseType<ErrorResponse>(404)]
    public async Task<ActionResult<PaperResponse>> GetPaper(Guid id)
    {
        var paper = await _paperService.GetPaperAsync(id);
        if (paper == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = "NotFound",
                Message = "Paper not found",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        _logger.LogInformation("Paper {PaperId} meta: Title='{Title}', Authors='{Authors}'", id, paper.Title, paper.Authors);

        return Ok(paper);
    }

    /// <summary>
    /// Get paper executive summary
    /// </summary>
    /// <param name="id">Paper ID</param>
    [HttpGet("{id}/summary")]
    [ProducesResponseType<SummaryResponse>(200)]
    [ProducesResponseType<ErrorResponse>(404)]
    public async Task<ActionResult<SummaryResponse>> GetSummary(Guid id)
    {
        var summary = await _paperService.GetSummaryAsync(id);
        if (summary == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = "NotFound",
                Message = "Summary not found",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(summary);
    }

    /// <summary>
    /// Get related work for paper
    /// </summary>
    /// <param name="id">Paper ID</param>
    [HttpGet("{id}/related")]
    [ProducesResponseType<RelatedResponse>(200)]
    [ProducesResponseType<ErrorResponse>(404)]
    public async Task<ActionResult<RelatedResponse>> GetRelated(Guid id)
    {
        var related = await _paperService.GetRelatedAsync(id);
        if (related == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = "NotFound",
                Message = "Related work not found",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(related);
    }

    /// <summary>
    /// Export paper as Markdown or PDF
    /// </summary>
    /// <param name="id">Paper ID</param>
    /// <param name="format">Export format: 'md' for Markdown, 'pdf' for PDF</param>
    [HttpPost("{id}/export")]
    [ProducesResponseType(200)]
    [ProducesResponseType<ErrorResponse>(400)]
    [ProducesResponseType<ErrorResponse>(404)]
    public async Task<IActionResult> ExportPaper(Guid id, [FromQuery] string format = "md")
    {
        try
        {
            var paper = await _paperService.GetPaperAsync(id);
            if (paper == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "NotFound",
                    Message = "Paper not found",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            byte[] content;
            string contentType;
            string fileName;

            switch (format.ToLowerInvariant())
            {
                case "md":
                    content = await _exporter.ExportMarkdownAsync(paper);
                    contentType = "text/markdown";
                    fileName = $"paper-{id}.md";
                    break;
                
                case "pdf":
                    content = await _exporter.ExportPdfAsync(paper);
                    contentType = "application/pdf";
                    fileName = $"paper-{id}.pdf";
                    break;
                
                case "pptx":
                    content = await _exporter.ExportPowerPointAsync(paper);
                    contentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    fileName = $"paper-{id}.html"; // For now, we'll generate HTML that can be opened in PowerPoint
                    break;
                
                default:
                    return BadRequest(new ErrorResponse
                    {
                        Error = "InvalidFormat",
                        Message = "Invalid export format. Use 'md', 'pdf', or 'pptx'.",
                        TraceId = HttpContext.TraceIdentifier
                    });
            }

            return File(content, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export paper {PaperId} as {Format}", id, format);
            return BadRequest(new ErrorResponse
            {
                Error = "ExportFailed",
                Message = "Failed to export paper",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
}

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet]
    [ProducesResponseType<HealthResponse>(200)]
    public ActionResult<HealthResponse> GetHealth()
    {
        return Ok(new HealthResponse { Status = "ok" });
    }
}
