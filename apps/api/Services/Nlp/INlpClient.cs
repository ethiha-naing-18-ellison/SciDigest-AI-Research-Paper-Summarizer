using Api.Models;

namespace Api.Services.Nlp;

public interface INlpClient
{
    Task<ParseResultDto> ParseAsync(Guid paperId, string filePath);
    Task<(string summary, string[] contributions, string[]? details, AnchorDto[] anchors, string abstract_text, string introduction, string methodology, string results, string discussion, string limitations, string technicalDetails, string impact)> SummarizeAsync(Guid paperId, SectionDto[] sections);
    Task<RelatedPayload> RelatedAsync(string? title, string[]? keyphrases, SectionDto[]? sections);
}
