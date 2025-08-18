namespace Api.Domain.Entities;

public class Summary
{
    public Guid Id { get; set; }
    
    public Guid PaperId { get; set; }
    
    public string ExecutiveSummary { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public Paper Paper { get; set; } = null!;
}
