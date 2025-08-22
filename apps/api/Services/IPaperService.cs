using Api.Domain.Entities;
using Api.Models;

namespace Api.Services;

public interface IPaperService
{
    Task<Guid> UploadAsync(IFormFile file);
    Task<PaperResponse?> GetPaperAsync(Guid id);
    Task<SummaryResponse?> GetSummaryAsync(Guid id);
    Task<RelatedResponse?> GetRelatedAsync(Guid id);
    Task EnqueueProcessingAsync(Guid paperId);
    Task<SearchResponse> SearchPapersAsync(SearchRequest request);
    Task<FilterOptionsResponse> GetFilterOptionsAsync();
}
