using Api.Domain.Entities;
using Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Persistence;

public interface IPaperRepository
{
    Task<Paper?> GetByIdAsync(Guid id);
    Task<Paper> CreateAsync(Paper paper);
    Task UpdateAsync(Paper paper);
    Task<PaperSection[]> GetSectionsAsync(Guid paperId);
    Task<Summary?> GetLatestSummaryAsync(Guid paperId);
    Task<Contribution?> GetLatestContributionsAsync(Guid paperId);
    Task<RelatedWork?> GetLatestRelatedWorkAsync(Guid paperId);
    Task SaveSectionsAsync(Guid paperId, IEnumerable<PaperSection> sections);
    Task SaveSummaryAsync(Summary summary);
    Task SaveContributionsAsync(Contribution contributions);
    Task SaveRelatedWorkAsync(RelatedWork relatedWork);
}

public class PaperRepository : IPaperRepository
{
    private readonly ResearchDbContext _context;

    public PaperRepository(ResearchDbContext context)
    {
        _context = context;
    }

    public async Task<Paper?> GetByIdAsync(Guid id)
    {
        return await _context.Papers
            .Include(p => p.Sections.OrderBy(s => s.OrderIdx))
            .Include(p => p.Summaries.OrderByDescending(s => s.CreatedAt))
            .Include(p => p.Contributions.OrderByDescending(c => c.CreatedAt))
            .Include(p => p.RelatedWorks.OrderBy(r => r.Provider == "OpenAlex" ? 0 : 1).ThenByDescending(r => r.CreatedAt))
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Paper> CreateAsync(Paper paper)
    {
        _context.Papers.Add(paper);
        await _context.SaveChangesAsync();
        return paper;
    }

    public async Task UpdateAsync(Paper paper)
    {
        paper.UpdatedAt = DateTime.UtcNow;
        _context.Papers.Update(paper);
        await _context.SaveChangesAsync();
    }

    public async Task<PaperSection[]> GetSectionsAsync(Guid paperId)
    {
        return await _context.PaperSections
            .Where(s => s.PaperId == paperId)
            .OrderBy(s => s.OrderIdx)
            .ToArrayAsync();
    }

    public async Task<Summary?> GetLatestSummaryAsync(Guid paperId)
    {
        return await _context.Summaries
            .Where(s => s.PaperId == paperId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Contribution?> GetLatestContributionsAsync(Guid paperId)
    {
        return await _context.Contributions
            .Where(c => c.PaperId == paperId)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<RelatedWork?> GetLatestRelatedWorkAsync(Guid paperId)
    {
        return await _context.RelatedWorks
            .Where(r => r.PaperId == paperId)
            .OrderBy(r => r.Provider == "OpenAlex" ? 0 : 1)
            .ThenByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task SaveSectionsAsync(Guid paperId, IEnumerable<PaperSection> sections)
    {
        // Remove existing sections
        var existingSections = await _context.PaperSections
            .Where(s => s.PaperId == paperId)
            .ToListAsync();
        
        _context.PaperSections.RemoveRange(existingSections);
        
        // Add new sections
        _context.PaperSections.AddRange(sections);
        
        await _context.SaveChangesAsync();
    }

    public async Task SaveSummaryAsync(Summary summary)
    {
        _context.Summaries.Add(summary);
        await _context.SaveChangesAsync();
    }

    public async Task SaveContributionsAsync(Contribution contributions)
    {
        _context.Contributions.Add(contributions);
        await _context.SaveChangesAsync();
    }

    public async Task SaveRelatedWorkAsync(RelatedWork relatedWork)
    {
        // Remove existing related work from the same provider
        var existing = await _context.RelatedWorks
            .Where(r => r.PaperId == relatedWork.PaperId && r.Provider == relatedWork.Provider)
            .ToListAsync();
        
        _context.RelatedWorks.RemoveRange(existing);
        
        // Add new related work
        _context.RelatedWorks.Add(relatedWork);
        
        await _context.SaveChangesAsync();
    }
}
