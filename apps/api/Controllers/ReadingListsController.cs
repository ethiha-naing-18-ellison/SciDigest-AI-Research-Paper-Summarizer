using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadingListsController : ControllerBase
{
    private readonly IReadingListService _readingListService;
    private readonly ILogger<ReadingListsController> _logger;

    public ReadingListsController(IReadingListService readingListService, ILogger<ReadingListsController> logger)
    {
        _readingListService = readingListService;
        _logger = logger;
    }

    /// <summary>
    /// Get all reading lists
    /// </summary>
    [HttpGet]
    [ProducesResponseType<ReadingListResponse[]>(200)]
    public async Task<ActionResult<ReadingListResponse[]>> GetAll()
    {
        var readingLists = await _readingListService.GetAllAsync();
        return Ok(readingLists);
    }

    /// <summary>
    /// Get a specific reading list by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType<ReadingListResponse>(200)]
    [ProducesResponseType<ErrorResponse>(404)]
    public async Task<ActionResult<ReadingListResponse>> GetById(Guid id)
    {
        var readingList = await _readingListService.GetByIdAsync(id);
        if (readingList == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = "NotFound",
                Message = "Reading list not found",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(readingList);
    }

    /// <summary>
    /// Create a new reading list
    /// </summary>
    [HttpPost]
    [ProducesResponseType<ReadingListResponse>(201)]
    [ProducesResponseType<ErrorResponse>(400)]
    public async Task<ActionResult<ReadingListResponse>> Create([FromBody] CreateReadingListRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "ValidationError",
                    Message = "Reading list name is required",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            var readingList = await _readingListService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = readingList.Id }, readingList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create reading list");
            return BadRequest(new ErrorResponse
            {
                Error = "CreateFailed",
                Message = "Failed to create reading list",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Update an existing reading list
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType<ReadingListResponse>(200)]
    [ProducesResponseType<ErrorResponse>(400)]
    [ProducesResponseType<ErrorResponse>(404)]
    public async Task<ActionResult<ReadingListResponse>> Update(Guid id, [FromBody] UpdateReadingListRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "ValidationError",
                    Message = "Reading list name is required",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            var readingList = await _readingListService.UpdateAsync(id, request);
            if (readingList == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "NotFound",
                    Message = "Reading list not found",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            return Ok(readingList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update reading list {ReadingListId}", id);
            return BadRequest(new ErrorResponse
            {
                Error = "UpdateFailed",
                Message = "Failed to update reading list",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Delete a reading list
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ErrorResponse>(404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _readingListService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new ErrorResponse
            {
                Error = "NotFound",
                Message = "Reading list not found",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Add a paper to a reading list
    /// </summary>
    [HttpPost("{id}/papers")]
    [ProducesResponseType<ReadingListItemResponse>(201)]
    [ProducesResponseType<ErrorResponse>(400)]
    [ProducesResponseType<ErrorResponse>(404)]
    public async Task<ActionResult<ReadingListItemResponse>> AddPaper(Guid id, [FromBody] AddPaperToListRequest request)
    {
        try
        {
            var item = await _readingListService.AddPaperAsync(id, request);
            if (item == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "NotFound",
                    Message = "Reading list not found",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            return CreatedAtAction(nameof(GetById), new { id }, item);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "DuplicatePaper",
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add paper to reading list {ReadingListId}", id);
            return BadRequest(new ErrorResponse
            {
                Error = "AddPaperFailed",
                Message = "Failed to add paper to reading list",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Remove a paper from a reading list
    /// </summary>
    [HttpDelete("{id}/papers/{paperId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ErrorResponse>(404)]
    public async Task<IActionResult> RemovePaper(Guid id, Guid paperId)
    {
        await _readingListService.RemovePaperAsync(id, paperId);
        return NoContent();
    }

    /// <summary>
    /// Update notes for a reading list item
    /// </summary>
    [HttpPut("items/{itemId}/notes")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ErrorResponse>(404)]
    public async Task<IActionResult> UpdateItemNotes(Guid itemId, [FromBody] UpdateItemNotesRequest request)
    {
        await _readingListService.UpdateItemNotesAsync(itemId, request);
        return NoContent();
    }

    /// <summary>
    /// Reorder items in a reading list
    /// </summary>
    [HttpPost("{id}/reorder")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ErrorResponse>(400)]
    public async Task<IActionResult> ReorderItems(Guid id, [FromBody] ReorderItemsRequest request)
    {
        try
        {
            await _readingListService.ReorderItemsAsync(id, request);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reorder items in reading list {ReadingListId}", id);
            return BadRequest(new ErrorResponse
            {
                Error = "ReorderFailed",
                Message = "Failed to reorder items",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
}
