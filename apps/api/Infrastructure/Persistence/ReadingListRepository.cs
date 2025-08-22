using Api.Domain.Entities;
using Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Persistence;

public interface IReadingListRepository
{
    Task<ReadingList[]> GetAllAsync();
    Task<ReadingList?> GetByIdAsync(Guid id);
    Task<ReadingList> CreateAsync(ReadingList readingList);
    Task UpdateAsync(ReadingList readingList);
    Task DeleteAsync(Guid id);
    Task<ReadingListItem> AddPaperAsync(Guid readingListId, Guid paperId, string? notes);
    Task RemovePaperAsync(Guid readingListId, Guid paperId);
    Task UpdateItemNotesAsync(Guid itemId, string? notes);
    Task ReorderItemsAsync(Guid readingListId, Dictionary<Guid, int> itemOrders);
}

public class ReadingListRepository : IReadingListRepository
{
    private readonly ResearchDbContext _context;

    public ReadingListRepository(ResearchDbContext context)
    {
        _context = context;
    }

    public async Task<ReadingList[]> GetAllAsync()
    {
        return await _context.ReadingLists
            .Include(rl => rl.Items.OrderBy(i => i.OrderIndex))
                .ThenInclude(i => i.Paper)
            .OrderByDescending(rl => rl.UpdatedAt)
            .ToArrayAsync();
    }

    public async Task<ReadingList?> GetByIdAsync(Guid id)
    {
        return await _context.ReadingLists
            .Include(rl => rl.Items.OrderBy(i => i.OrderIndex))
                .ThenInclude(i => i.Paper)
            .FirstOrDefaultAsync(rl => rl.Id == id);
    }

    public async Task<ReadingList> CreateAsync(ReadingList readingList)
    {
        readingList.CreatedAt = DateTime.UtcNow;
        readingList.UpdatedAt = DateTime.UtcNow;
        
        _context.ReadingLists.Add(readingList);
        await _context.SaveChangesAsync();
        
        return readingList;
    }

    public async Task UpdateAsync(ReadingList readingList)
    {
        readingList.UpdatedAt = DateTime.UtcNow;
        _context.ReadingLists.Update(readingList);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var readingList = await _context.ReadingLists.FindAsync(id);
        if (readingList != null)
        {
            _context.ReadingLists.Remove(readingList);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ReadingListItem> AddPaperAsync(Guid readingListId, Guid paperId, string? notes)
    {
        // Check if paper is already in the reading list
        var existingItem = await _context.ReadingListItems
            .FirstOrDefaultAsync(i => i.ReadingListId == readingListId && i.PaperId == paperId);
        
        if (existingItem != null)
        {
            throw new InvalidOperationException("Paper is already in this reading list");
        }

        // Get the next order index
        var maxOrder = await _context.ReadingListItems
            .Where(i => i.ReadingListId == readingListId)
            .MaxAsync(i => (int?)i.OrderIndex) ?? -1;

        var item = new ReadingListItem
        {
            Id = Guid.NewGuid(),
            ReadingListId = readingListId,
            PaperId = paperId,
            Notes = notes,
            AddedAt = DateTime.UtcNow,
            OrderIndex = maxOrder + 1
        };

        _context.ReadingListItems.Add(item);

        // Update the reading list's UpdatedAt timestamp
        var readingList = await _context.ReadingLists.FindAsync(readingListId);
        if (readingList != null)
        {
            readingList.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        
        return item;
    }

    public async Task RemovePaperAsync(Guid readingListId, Guid paperId)
    {
        var item = await _context.ReadingListItems
            .FirstOrDefaultAsync(i => i.ReadingListId == readingListId && i.PaperId == paperId);
        
        if (item != null)
        {
            _context.ReadingListItems.Remove(item);

            // Update the reading list's UpdatedAt timestamp
            var readingList = await _context.ReadingLists.FindAsync(readingListId);
            if (readingList != null)
            {
                readingList.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateItemNotesAsync(Guid itemId, string? notes)
    {
        var item = await _context.ReadingListItems
            .Include(i => i.ReadingList)
            .FirstOrDefaultAsync(i => i.Id == itemId);
        
        if (item != null)
        {
            item.Notes = notes;
            item.ReadingList.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ReorderItemsAsync(Guid readingListId, Dictionary<Guid, int> itemOrders)
    {
        var items = await _context.ReadingListItems
            .Where(i => i.ReadingListId == readingListId)
            .ToListAsync();

        foreach (var item in items)
        {
            if (itemOrders.TryGetValue(item.Id, out var newOrder))
            {
                item.OrderIndex = newOrder;
            }
        }

        // Update the reading list's UpdatedAt timestamp
        var readingList = await _context.ReadingLists.FindAsync(readingListId);
        if (readingList != null)
        {
            readingList.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}
