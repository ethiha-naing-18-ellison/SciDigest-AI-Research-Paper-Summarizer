using System.ComponentModel.DataAnnotations;

namespace Api.Domain.Entities;

public class ProcessingJob
{
    public Guid Id { get; set; }
    
    public Guid PaperId { get; set; }
    
    [MaxLength(32)]
    public string Stage { get; set; } = string.Empty;
    
    [MaxLength(16)]
    public string State { get; set; } = string.Empty;
    
    public DateTime StartedAt { get; set; }
    
    public DateTime? FinishedAt { get; set; }
    
    public string? Error { get; set; }
    
    // Navigation property
    public Paper Paper { get; set; } = null!;
}
