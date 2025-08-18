using Api.Domain.Entities;
using Api.Infrastructure.Persistence;
using Api.Infrastructure.Storage;
using Api.Models;
using Api.Services.Processing;
using AutoMapper;
using Hangfire;

namespace Api.Services;

public class PaperService : IPaperService
{
    private readonly IPaperRepository _repository;
    private readonly IFileStorage _fileStorage;
    private readonly IMapper _mapper;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<PaperService> _logger;

    public PaperService(
        IPaperRepository repository,
        IFileStorage fileStorage,
        IMapper mapper,
        IBackgroundJobClient backgroundJobClient,
        ILogger<PaperService> logger)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _mapper = mapper;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public async Task<Guid> UploadAsync(IFormFile file)
    {
        var paperId = Guid.NewGuid();
        
        try
        {
            // Save file to storage
            using var stream = file.OpenReadStream();
            var filePath = await _fileStorage.SaveAsync(paperId, stream);
            
            // Create paper entity
            var paper = new Paper
            {
                Id = paperId,
                FilePath = filePath,
                Pages = 0, // Will be updated during processing
                Status = PaperStatus.Uploaded,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            await _repository.CreateAsync(paper);
            
            // Enqueue processing
            await EnqueueProcessingAsync(paperId);
            
            _logger.LogInformation("Paper {PaperId} uploaded successfully", paperId);
            return paperId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload paper {PaperId}", paperId);
            
            // Cleanup file if it was saved
            if (_fileStorage.Exists(paperId))
            {
                try
                {
                    File.Delete(_fileStorage.GetPath(paperId));
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning(cleanupEx, "Failed to cleanup file for paper {PaperId}", paperId);
                }
            }
            
            throw;
        }
    }

    public async Task<PaperResponse?> GetPaperAsync(Guid id)
    {
        var paper = await _repository.GetByIdAsync(id);
        return paper == null ? null : _mapper.Map<PaperResponse>(paper);
    }

    public async Task<SummaryResponse?> GetSummaryAsync(Guid id)
    {
        var summary = await _repository.GetLatestSummaryAsync(id);
        return summary == null ? null : _mapper.Map<SummaryResponse>(summary);
    }

    public async Task<RelatedResponse?> GetRelatedAsync(Guid id)
    {
        var related = await _repository.GetLatestRelatedWorkAsync(id);
        return related == null ? null : _mapper.Map<RelatedResponse>(related);
    }

    public Task EnqueueProcessingAsync(Guid paperId)
    {
        _backgroundJobClient.Enqueue<ProcessingPipeline>(x => x.ProcessAsync(paperId));
        _logger.LogInformation("Enqueued processing for paper {PaperId}", paperId);
        return Task.CompletedTask;
    }
}
