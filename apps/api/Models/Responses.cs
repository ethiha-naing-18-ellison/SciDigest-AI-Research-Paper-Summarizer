namespace Api.Models;

public class PaperResponse
{
    public string Id { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Authors { get; set; }
    public int? Year { get; set; }
    public string? Venue { get; set; }
    public int Pages { get; set; }
    public string Status { get; set; } = string.Empty;
    public PaperSectionResponse[]? Sections { get; set; }
    public SummaryResponse? Summary { get; set; }
    public ContributionsResponse? Contributions { get; set; }
    public RelatedResponse? Related { get; set; }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
}

public class HealthResponse
{
    public string Status { get; set; } = "ok";
}
