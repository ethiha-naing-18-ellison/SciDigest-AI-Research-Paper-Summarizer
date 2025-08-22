using Api.Domain.Entities;
using Api.Infrastructure.Data;
using Api.Infrastructure.Persistence;
using Api.Models;
using Api.Services.Nlp;
using Api.Services.DocumentConverter;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;

namespace Api.Services.Processing;

public class ProcessingPipeline
{
    private readonly ResearchDbContext _context;
    private readonly IPaperRepository _repository;
    private readonly INlpClient _nlpClient;
    private readonly IDocumentConverterService _documentConverter;
    private readonly ILogger<ProcessingPipeline> _logger;

    public ProcessingPipeline(
        ResearchDbContext context,
        IPaperRepository repository,
        INlpClient nlpClient,
        IDocumentConverterService documentConverter,
        ILogger<ProcessingPipeline> logger)
    {
        _context = context;
        _repository = repository;
        _nlpClient = nlpClient;
        _documentConverter = documentConverter;
        _logger = logger;
    }

    public async Task ProcessAsync(Guid paperId)
    {
        var paper = await _repository.GetByIdAsync(paperId);
        if (paper == null)
        {
            _logger.LogError("Paper {PaperId} not found", paperId);
            return;
        }

        try
        {
            // Update paper status
            paper.Status = PaperStatus.Processing;
            await _repository.UpdateAsync(paper);

            // Create processing jobs
            await CreateProcessingJobAsync(paperId, "parse", "queued");
            await CreateProcessingJobAsync(paperId, "summarize", "queued");
            await CreateProcessingJobAsync(paperId, "related", "queued");

            // Step 1: Parse
            await ProcessParseStageAsync(paperId, paper.FilePath!);

            // Step 2: Summarize
            await ProcessSummarizeStageAsync(paperId);

            // Step 3: Related works
            await ProcessRelatedStageAsync(paperId);

            // Mark paper as completed
            paper.Status = PaperStatus.Completed;
            await _repository.UpdateAsync(paper);

            _logger.LogInformation("Successfully processed paper {PaperId}", paperId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process paper {PaperId}", paperId);
            
            // Mark paper as failed
            paper.Status = PaperStatus.Failed;
            await _repository.UpdateAsync(paper);
            
            // Update failed job
            await UpdateProcessingJobErrorAsync(paperId, ex.Message);
        }
    }

    private async Task ProcessParseStageAsync(Guid paperId, string filePath)
    {
        await UpdateProcessingJobAsync(paperId, "parse", "running");

        try
        {
            // Check if we need to convert the document first
            var fileName = Path.GetFileName(filePath);
            var contentType = GetContentTypeFromExtension(Path.GetExtension(fileName));
            
            ParseResultDto parse;
            
            if (_documentConverter.CanConvert(fileName, contentType))
            {
                // Convert document to text first
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var convertedDocument = await _documentConverter.ConvertAsync(fileStream, fileName, contentType);
                
                // For now, we'll create a simple parse result from the converted text
                // In the future, we could enhance the NLP service to accept text directly
                parse = CreateParseResultFromText(paperId, convertedDocument.Text, fileName);
            }
            else
            {
                // Use existing NLP service for PDF files
                parse = await _nlpClient.ParseAsync(paperId, filePath);
            }
            
            // Update paper metadata if present
            var paper = await _repository.GetByIdAsync(paperId);
            if (paper != null && parse.Meta is { } m)
            {
                if (!string.IsNullOrWhiteSpace(m.Title)) paper.Title = m.Title;
                if (!string.IsNullOrWhiteSpace(m.Authors)) paper.Authors = m.Authors;
                if (m.Year.HasValue) paper.Year = m.Year;
                if (!string.IsNullOrWhiteSpace(m.Venue)) paper.Venue = m.Venue;
                paper.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(paper);
            }
            
            // Convert sections to entities
            var sectionEntities = parse.Sections.Select(s => new PaperSection
            {
                Id = Guid.NewGuid(),
                PaperId = paperId,
                Name = s.Name,
                OrderIdx = s.OrderIdx,
                Text = s.Text,
                Tokens = s.Tokens,
                PageStart = s.PageStart,
                PageEnd = s.PageEnd
            });

            await _repository.SaveSectionsAsync(paperId, sectionEntities);
            
            // Update paper pages count
            if (paper != null && parse.Sections.Length > 0)
            {
                paper.Pages = parse.Sections.Max(s => s.PageEnd);
                await _repository.UpdateAsync(paper);
            }

            await UpdateProcessingJobAsync(paperId, "parse", "done");
            _logger.LogInformation("Parse stage completed for paper {PaperId}", paperId);
        }
        catch (Exception ex)
        {
            await UpdateProcessingJobAsync(paperId, "parse", "failed", ex.Message);
            throw;
        }
    }

    private async Task ProcessSummarizeStageAsync(Guid paperId)
    {
        await UpdateProcessingJobAsync(paperId, "summarize", "running");

        try
        {
            var sections = await _repository.GetSectionsAsync(paperId);
            var sectionDtos = sections.Select(s => new SectionDto
            {
                Name = s.Name,
                Text = s.Text,
                Tokens = s.Tokens,
                PageStart = s.PageStart,
                PageEnd = s.PageEnd,
                OrderIdx = s.OrderIdx
            }).ToArray();

            var (summary, contributions, details, anchors, abstract_text, introduction, methodology, results, discussion, limitations, technicalDetails, impact) = await _nlpClient.SummarizeAsync(paperId, sectionDtos);

            // Save summary with comprehensive sections
            var summaryEntity = new Summary
            {
                Id = Guid.NewGuid(),
                PaperId = paperId,
                ExecutiveSummary = summary,
                Abstract = abstract_text,
                Introduction = introduction,
                Methodology = methodology,
                Results = results,
                Discussion = discussion,
                Limitations = limitations,
                TechnicalDetails = technicalDetails,
                Impact = impact,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.SaveSummaryAsync(summaryEntity);

            // Save contributions with details and anchors
            var items = contributions
                .Select((headline, i) => new {
                    headline,
                    detail = (details != null && i < details.Length) ? details[i] : null
                }).ToArray();

            var contributionEntity = new Contribution
            {
                Id = Guid.NewGuid(),
                PaperId = paperId,
                BulletsJson = JsonSerializer.Serialize(items),
                CreatedAt = DateTime.UtcNow
            };
            await _repository.SaveContributionsAsync(contributionEntity);

            await UpdateProcessingJobAsync(paperId, "summarize", "done");
            _logger.LogInformation("Summarize stage completed for paper {PaperId}", paperId);
        }
        catch (Exception ex)
        {
            await UpdateProcessingJobAsync(paperId, "summarize", "failed", ex.Message);
            throw;
        }
    }

    private async Task ProcessRelatedStageAsync(Guid paperId)
    {
        await UpdateProcessingJobAsync(paperId, "related", "running");

        try
        {
            var paper = await _repository.GetByIdAsync(paperId);
            var sections = await _repository.GetSectionsAsync(paperId);
            
            var sectionDtos = sections.Select(s => new SectionDto
            {
                Name = s.Name,
                Text = s.Text,
                Tokens = s.Tokens,
                PageStart = s.PageStart,
                PageEnd = s.PageEnd,
                OrderIdx = s.OrderIdx
            }).ToArray();

            // Extract keyphrases from title and abstract (simple implementation)
            var keyphrases = ExtractKeyphrases(paper?.Title, sections.FirstOrDefault(s => s.Name.ToLower() == "abstract")?.Text);

            var relatedPayload = await _nlpClient.RelatedAsync(paper?.Title, keyphrases, sectionDtos);

            var relatedEntity = new RelatedWork
            {
                Id = Guid.NewGuid(),
                PaperId = paperId,
                Provider = relatedPayload.Provider,
                ResultsJson = JsonSerializer.Serialize(relatedPayload.Items),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30) // Cache for 30 days
            };

            await _repository.SaveRelatedWorkAsync(relatedEntity);

            await UpdateProcessingJobAsync(paperId, "related", "done");
            _logger.LogInformation("Related stage completed for paper {PaperId}", paperId);
        }
        catch (Exception ex)
        {
            await UpdateProcessingJobAsync(paperId, "related", "failed", ex.Message);
            throw;
        }
    }

    private async Task CreateProcessingJobAsync(Guid paperId, string stage, string state)
    {
        var job = new ProcessingJob
        {
            Id = Guid.NewGuid(),
            PaperId = paperId,
            Stage = stage,
            State = state,
            StartedAt = DateTime.UtcNow
        };

        _context.ProcessingJobs.Add(job);
        await _context.SaveChangesAsync();
    }

    private async Task UpdateProcessingJobAsync(Guid paperId, string stage, string state, string? error = null)
    {
        var job = await _context.ProcessingJobs
            .FirstOrDefaultAsync(j => j.PaperId == paperId && j.Stage == stage);

        if (job != null)
        {
            job.State = state;
            job.Error = error;
            
            if (state == "done" || state == "failed")
            {
                job.FinishedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }

    private async Task UpdateProcessingJobErrorAsync(Guid paperId, string error)
    {
        var runningJob = await _context.ProcessingJobs
            .Where(j => j.PaperId == paperId && j.State == "running")
            .FirstOrDefaultAsync();

        if (runningJob != null)
        {
            runningJob.State = "failed";
            runningJob.Error = error;
            runningJob.FinishedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private static string[] ExtractKeyphrases(string? title, string? abstractText)
    {
        var phrases = new List<string>();
        
        if (!string.IsNullOrEmpty(title))
        {
            // Simple keyword extraction from title
            var titleWords = title.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3)
                .Take(5);
            phrases.AddRange(titleWords);
        }

        if (!string.IsNullOrEmpty(abstractText))
        {
            // Simple keyword extraction from abstract
            var abstractWords = abstractText.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 4)
                .Take(10);
            phrases.AddRange(abstractWords);
        }

        return phrases.Distinct().ToArray();
    }

    private static string GetContentTypeFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".html" => "text/html",
            ".htm" => "text/html",
            ".xhtml" => "application/xhtml+xml",
            ".tex" => "text/x-tex",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    private static ParseResultDto CreateParseResultFromText(Guid paperId, string text, string fileName)
    {
        // Split text into sections (simple approach)
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var sections = new List<SectionDto>();
        
        var currentSection = new StringBuilder();
        var sectionIndex = 0;
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine)) continue;
            
            // Simple heuristic: if line is all caps and short, it might be a section header
            if (trimmedLine.Length < 100 && trimmedLine == trimmedLine.ToUpperInvariant())
            {
                // Save previous section if it has content
                if (currentSection.Length > 0)
                {
                    sections.Add(new SectionDto
                    {
                        Name = $"section_{sectionIndex}",
                        Text = currentSection.ToString().Trim(),
                        Tokens = currentSection.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                        PageStart = sectionIndex + 1,
                        PageEnd = sectionIndex + 1,
                        OrderIdx = sectionIndex
                    });
                    sectionIndex++;
                    currentSection.Clear();
                }
            }
            
            currentSection.AppendLine(trimmedLine);
        }
        
        // Add the last section
        if (currentSection.Length > 0)
        {
            sections.Add(new SectionDto
            {
                Name = $"section_{sectionIndex}",
                Text = currentSection.ToString().Trim(),
                Tokens = currentSection.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                PageStart = sectionIndex + 1,
                PageEnd = sectionIndex + 1,
                OrderIdx = sectionIndex
            });
        }
        
        // If no sections were created, create one with all text
        if (sections.Count == 0)
        {
            sections.Add(new SectionDto
            {
                Name = "content",
                Text = text,
                Tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                PageStart = 1,
                PageEnd = 1,
                OrderIdx = 0
            });
        }
        
        return new ParseResultDto(
            new ParseMetaDto(
                Path.GetFileNameWithoutExtension(fileName),
                "",
                null,
                ""
            ),
            sections.ToArray()
        );
    }
}
