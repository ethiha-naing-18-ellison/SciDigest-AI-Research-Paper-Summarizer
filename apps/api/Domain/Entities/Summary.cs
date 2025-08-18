namespace Api.Domain.Entities;

public class Summary
{
    public Guid Id { get; set; }
    
    public Guid PaperId { get; set; }
    
    public string ExecutiveSummary { get; set; } = string.Empty;
    
    // New comprehensive sections
    public string Abstract { get; set; } = string.Empty;
    public string Introduction { get; set; } = string.Empty;
    public string Methodology { get; set; } = string.Empty;
    public string Results { get; set; } = string.Empty;
    public string Discussion { get; set; } = string.Empty;
    public string Limitations { get; set; } = string.Empty;
    public string TechnicalDetails { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public Paper Paper { get; set; } = null!;
}
