using Api.Domain.Entities;
using Api.Infrastructure.Persistence;
using Api.Models;
using AutoMapper;

namespace Api.Services;

public interface IReadingListService
{
    Task<ReadingListResponse[]> GetAllAsync();
    Task<ReadingListResponse?> GetByIdAsync(Guid id);
    Task<ReadingListResponse> CreateAsync(CreateReadingListRequest request);
    Task<ReadingListResponse?> UpdateAsync(Guid id, UpdateReadingListRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<ReadingListItemResponse?> AddPaperAsync(Guid readingListId, AddPaperToListRequest request);
    Task<bool> RemovePaperAsync(Guid readingListId, Guid paperId);
    Task<bool> UpdateItemNotesAsync(Guid itemId, UpdateItemNotesRequest request);
    Task<bool> ReorderItemsAsync(Guid readingListId, ReorderItemsRequest request);
}

public class ReadingListService : IReadingListService
{
    private readonly IReadingListRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ReadingListService> _logger;

    public ReadingListService(
        IReadingListRepository repository,
        IMapper mapper,
        ILogger<ReadingListService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReadingListResponse[]> GetAllAsync()
    {
        var readingLists = await _repository.GetAllAsync();
        return _mapper.Map<ReadingListResponse[]>(readingLists);
    }

    public async Task<ReadingListResponse?> GetByIdAsync(Guid id)
    {
        var readingList = await _repository.GetByIdAsync(id);
        return readingList == null ? null : _mapper.Map<ReadingListResponse>(readingList);
    }

    public async Task<ReadingListResponse> CreateAsync(CreateReadingListRequest request)
    {
        var readingList = new ReadingList
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim()
        };

        var created = await _repository.CreateAsync(readingList);
        _logger.LogInformation("Created reading list {ReadingListId} with name '{Name}'", created.Id, created.Name);
        
        return _mapper.Map<ReadingListResponse>(created);
    }

    public async Task<ReadingListResponse?> UpdateAsync(Guid id, UpdateReadingListRequest request)
    {
        var readingList = await _repository.GetByIdAsync(id);
        if (readingList == null)
        {
            return null;
        }

        readingList.Name = request.Name.Trim();
        readingList.Description = request.Description?.Trim();

        await _repository.UpdateAsync(readingList);
        _logger.LogInformation("Updated reading list {ReadingListId}", id);

        return _mapper.Map<ReadingListResponse>(readingList);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var readingList = await _repository.GetByIdAsync(id);
        if (readingList == null)
        {
            return false;
        }

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Deleted reading list {ReadingListId}", id);
        
        return true;
    }

    public async Task<ReadingListItemResponse?> AddPaperAsync(Guid readingListId, AddPaperToListRequest request)
    {
        try
        {
            if (!Guid.TryParse(request.PaperId, out var paperId))
            {
                throw new ArgumentException("Invalid paper ID format");
            }

            var item = await _repository.AddPaperAsync(readingListId, paperId, request.Notes?.Trim());
            _logger.LogInformation("Added paper {PaperId} to reading list {ReadingListId}", paperId, readingListId);
            
            return _mapper.Map<ReadingListItemResponse>(item);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to add paper to reading list: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> RemovePaperAsync(Guid readingListId, Guid paperId)
    {
        await _repository.RemovePaperAsync(readingListId, paperId);
        _logger.LogInformation("Removed paper {PaperId} from reading list {ReadingListId}", paperId, readingListId);
        
        return true;
    }

    public async Task<bool> UpdateItemNotesAsync(Guid itemId, UpdateItemNotesRequest request)
    {
        await _repository.UpdateItemNotesAsync(itemId, request.Notes?.Trim());
        _logger.LogInformation("Updated notes for reading list item {ItemId}", itemId);
        
        return true;
    }

    public async Task<bool> ReorderItemsAsync(Guid readingListId, ReorderItemsRequest request)
    {
        var itemOrders = new Dictionary<Guid, int>();
        
        foreach (var kvp in request.ItemOrders)
        {
            if (Guid.TryParse(kvp.Key, out var itemId))
            {
                itemOrders[itemId] = kvp.Value;
            }
        }

        await _repository.ReorderItemsAsync(readingListId, itemOrders);
        _logger.LogInformation("Reordered items in reading list {ReadingListId}", readingListId);
        
        return true;
    }
}
