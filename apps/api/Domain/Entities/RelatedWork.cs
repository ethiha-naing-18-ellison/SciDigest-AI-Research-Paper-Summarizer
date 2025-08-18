using System.ComponentModel.DataAnnotations;

namespace Api.Domain.Entities;

public class RelatedWork
{
    public Guid Id { get; set; }
    
    public Guid PaperId { get; set; }
    
    [MaxLength(32)]
    public string Provider { get; set; } = string.Empty;
    
    public string ResultsJson { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
    
    // Navigation property
    public Paper Paper { get; set; } = null!;
}
