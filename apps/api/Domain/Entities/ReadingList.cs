using System.ComponentModel.DataAnnotations;

namespace Api.Domain.Entities;

public class ReadingList
{
    public Guid Id { get; set; }
    
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1024)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<ReadingListItem> Items { get; set; } = new List<ReadingListItem>();
}

public class ReadingListItem
{
    public Guid Id { get; set; }
    
    public Guid ReadingListId { get; set; }
    
    public Guid PaperId { get; set; }
    
    [MaxLength(1024)]
    public string? Notes { get; set; }
    
    public DateTime AddedAt { get; set; }
    
    public int OrderIndex { get; set; }
    
    // Navigation properties
    public ReadingList ReadingList { get; set; } = null!;
    public Paper Paper { get; set; } = null!;
}
