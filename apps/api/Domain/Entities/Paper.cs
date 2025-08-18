using System.ComponentModel.DataAnnotations;

namespace Api.Domain.Entities;

public class Paper
{
    public Guid Id { get; set; }
    
    [MaxLength(512)]
    public string? Title { get; set; }
    
    [MaxLength(512)]
    public string? Authors { get; set; }
    
    public int? Year { get; set; }
    
    [MaxLength(256)]
    public string? Venue { get; set; }
    
    [MaxLength(1024)]
    public string? FilePath { get; set; }
    
    public int Pages { get; set; }
    
    public PaperStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<PaperSection> Sections { get; set; } = new List<PaperSection>();
    public ICollection<Summary> Summaries { get; set; } = new List<Summary>();
    public ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();
    public ICollection<RelatedWork> RelatedWorks { get; set; } = new List<RelatedWork>();
    public ICollection<ProcessingJob> ProcessingJobs { get; set; } = new List<ProcessingJob>();
}
