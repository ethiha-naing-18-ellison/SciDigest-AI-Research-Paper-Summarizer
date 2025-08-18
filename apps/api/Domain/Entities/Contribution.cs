namespace Api.Domain.Entities;

public class Contribution
{
    public Guid Id { get; set; }
    
    public Guid PaperId { get; set; }
    
    public string BulletsJson { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public Paper Paper { get; set; } = null!;
}
