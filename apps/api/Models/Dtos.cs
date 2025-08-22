using Api.Domain.Entities;

namespace Api.Models;

public sealed record ParseMetaDto(string? Title, string? Authors, int? Year, string? Venue);

public sealed record ParseResultDto(ParseMetaDto? Meta, SectionDto[] Sections);

public class SectionDto
{
    public string Name { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Tokens { get; set; }
    public int PageStart { get; set; }
    public int PageEnd { get; set; }
    public int OrderIdx { get; set; }
}

public class AnchorDto
{
    public int BulletIndex { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public int PageStart { get; set; }
    public int PageEnd { get; set; }
}

public class RelatedPayload
{
    public string Provider { get; set; } = string.Empty;
    public List<RelatedItem> Items { get; set; } = new();
}

public class RelatedItem
{
    public string Title { get; set; } = string.Empty;
    public string Authors { get; set; } = string.Empty;
    public string? Venue { get; set; }
    public int? Year { get; set; }
    public string? Url { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<string>? AlternativeUrls { get; set; }  // Additional URLs for the same paper
}

public class PaperSectionResponse
{
    public string Name { get; set; } = string.Empty;
    public int OrderIdx { get; set; }
    public int Tokens { get; set; }
    public int PageStart { get; set; }
    public int PageEnd { get; set; }
}

public class SummaryResponse
{
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string Introduction { get; set; } = string.Empty;
    public string Methodology { get; set; } = string.Empty;
    public string Results { get; set; } = string.Empty;
    public string Discussion { get; set; } = string.Empty;
    public string Limitations { get; set; } = string.Empty;
    public string TechnicalDetails { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
}

public class ContributionsResponse
{
    public string[] Bullets { get; set; } = Array.Empty<string>();
    public string[]? Details { get; set; }
    public AnchorDto[] Anchors { get; set; } = Array.Empty<AnchorDto>();
}

public class RelatedResponse
{
    public string Provider { get; set; } = string.Empty;
    public RelatedItem[] Items { get; set; } = Array.Empty<RelatedItem>();
}

public class SearchRequest
{
    public string? SearchTerm { get; set; }
    public string? Venue { get; set; }
    public int? Year { get; set; }
    public PaperStatus? Status { get; set; }
    public string? SortBy { get; set; } = "createdAt";
    public bool SortDescending { get; set; } = true;
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
}

public class SearchResponse
{
    public PaperResponse[] Papers { get; set; } = Array.Empty<PaperResponse>();
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}

public class FilterOptionsResponse
{
    public string[] Venues { get; set; } = Array.Empty<string>();
    public int[] Years { get; set; } = Array.Empty<int>();
    public PaperStatus[] Statuses { get; set; } = Array.Empty<PaperStatus>();
}

// Reading List DTOs
public class CreateReadingListRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateReadingListRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class AddPaperToListRequest
{
    public string PaperId { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateItemNotesRequest
{
    public string? Notes { get; set; }
}

public class ReorderItemsRequest
{
    public Dictionary<string, int> ItemOrders { get; set; } = new();
}

public class ReadingListResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ReadingListItemResponse[] Items { get; set; } = Array.Empty<ReadingListItemResponse>();
}

public class ReadingListItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string PaperId { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime AddedAt { get; set; }
    public int OrderIndex { get; set; }
    public PaperResponse? Paper { get; set; }
}