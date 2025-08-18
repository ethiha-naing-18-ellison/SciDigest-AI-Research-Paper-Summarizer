using Api.Domain.Entities;
using Api.Infrastructure.Data;
using Api.Infrastructure.Persistence;
using Api.Models;
using Api.Services.Nlp;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Api.Services.Processing;

public class ProcessingPipeline
{
    private readonly ResearchDbContext _context;
    private readonly IPaperRepository _repository;
    private readonly INlpClient _nlpClient;
    private readonly ILogger<ProcessingPipeline> _logger;

    public ProcessingPipeline(
        ResearchDbContext context,
        IPaperRepository repository,
        INlpClient nlpClient,
        ILogger<ProcessingPipeline> logger)
    {
        _context = context;
        _repository = repository;
        _nlpClient = nlpClient;
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
            var parse = await _nlpClient.ParseAsync(paperId, filePath);
            
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

            var (summary, contributions, anchors) = await _nlpClient.SummarizeAsync(paperId, sectionDtos);

            // Save summary
            var summaryEntity = new Summary
            {
                Id = Guid.NewGuid(),
                PaperId = paperId,
                ExecutiveSummary = summary,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.SaveSummaryAsync(summaryEntity);

            // Save contributions with anchors
            var contributionData = new ContributionData
            {
                Bullets = contributions,
                Anchors = anchors
            };

            var contributionEntity = new Contribution
            {
                Id = Guid.NewGuid(),
                PaperId = paperId,
                BulletsJson = JsonSerializer.Serialize(contributionData),
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
}
