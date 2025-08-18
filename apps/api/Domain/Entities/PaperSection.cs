using System.ComponentModel.DataAnnotations;

namespace Api.Domain.Entities;

public class PaperSection
{
    public Guid Id { get; set; }
    
    public Guid PaperId { get; set; }
    
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;
    
    public int OrderIdx { get; set; }
    
    public string Text { get; set; } = string.Empty;
    
    public int Tokens { get; set; }
    
    public int PageStart { get; set; }
    
    public int PageEnd { get; set; }
    
    // Navigation property
    public Paper Paper { get; set; } = null!;
}
