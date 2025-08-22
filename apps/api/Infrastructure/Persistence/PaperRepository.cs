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
    Task<(Paper[] papers, int totalCount)> SearchPapersAsync(string? searchTerm, string? venue, int? year, PaperStatus? status, string? sortBy, bool sortDescending, int skip, int take);
    Task<string[]> GetDistinctVenuesAsync();
    Task<int[]> GetDistinctYearsAsync();
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

    public async Task<(Paper[] papers, int totalCount)> SearchPapersAsync(string? searchTerm, string? venue, int? year, PaperStatus? status, string? sortBy, bool sortDescending, int skip, int take)
    {
        var query = _context.Papers
            .Include(p => p.Summaries.OrderByDescending(s => s.CreatedAt))
            .Include(p => p.Contributions.OrderByDescending(c => c.CreatedAt))
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(p => 
                (p.Title != null && p.Title.ToLower().Contains(lowerSearchTerm)) ||
                (p.Authors != null && p.Authors.ToLower().Contains(lowerSearchTerm)) ||
                p.Summaries.Any(s => s.ExecutiveSummary.ToLower().Contains(lowerSearchTerm)) ||
                p.Contributions.Any(c => c.BulletsJson.ToLower().Contains(lowerSearchTerm))
            );
        }

        if (!string.IsNullOrEmpty(venue))
        {
            query = query.Where(p => p.Venue != null && p.Venue.ToLower().Contains(venue.ToLower()));
        }

        if (year.HasValue)
        {
            query = query.Where(p => p.Year == year.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        switch (sortBy?.ToLower())
        {
            case "title":
                query = sortDescending 
                    ? query.OrderByDescending(p => p.Title)
                    : query.OrderBy(p => p.Title);
                break;
            case "authors":
                query = sortDescending 
                    ? query.OrderByDescending(p => p.Authors)
                    : query.OrderBy(p => p.Authors);
                break;
            case "year":
                query = sortDescending 
                    ? query.OrderByDescending(p => p.Year)
                    : query.OrderBy(p => p.Year);
                break;
            case "venue":
                query = sortDescending 
                    ? query.OrderByDescending(p => p.Venue)
                    : query.OrderBy(p => p.Venue);
                break;
            case "createdat":
            default:
                query = sortDescending 
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt);
                break;
        }

        // Apply pagination
        var papers = await query
            .Skip(skip)
            .Take(take)
            .ToArrayAsync();

        return (papers, totalCount);
    }

    public async Task<string[]> GetDistinctVenuesAsync()
    {
        return await _context.Papers
            .Where(p => p.Venue != null && p.Venue != "")
            .Select(p => p.Venue!)
            .Distinct()
            .OrderBy(v => v)
            .ToArrayAsync();
    }

    public async Task<int[]> GetDistinctYearsAsync()
    {
        return await _context.Papers
            .Where(p => p.Year.HasValue)
            .Select(p => p.Year!.Value)
            .Distinct()
            .OrderByDescending(y => y)
            .ToArrayAsync();
    }
}
