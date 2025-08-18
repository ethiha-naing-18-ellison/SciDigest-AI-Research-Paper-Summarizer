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
