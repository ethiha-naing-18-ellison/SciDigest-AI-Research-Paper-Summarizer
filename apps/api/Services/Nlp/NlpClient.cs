using Api.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Api.Services.Nlp;

public class NlpClient : INlpClient
{
    private readonly HttpClient _httpClient;
    private readonly NlpOptions _options;
    private readonly ILogger<NlpClient> _logger;

    public NlpClient(HttpClient httpClient, IOptions<NlpOptions> options, ILogger<NlpClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<SectionDto[]> ParseAsync(Guid paperId, string filePath)
    {
        if (_options.Stub)
        {
            return GenerateStubSections(paperId);
        }

        try
        {
            var request = new { paperId = paperId.ToString(), filePath };
            var response = await _httpClient.PostAsJsonAsync("/parse", request);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SectionDto[]>(content) ?? Array.Empty<SectionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse paper {PaperId}", paperId);
            throw;
        }
    }

    public async Task<(string summary, string[] contributions, AnchorDto[] anchors)> SummarizeAsync(Guid paperId, SectionDto[] sections)
    {
        if (_options.Stub)
        {
            return GenerateStubSummary(paperId);
        }

        try
        {
            var request = new { paperId = paperId.ToString(), sections };
            var response = await _httpClient.PostAsJsonAsync("/summarize", request);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SummarizeResponse>(content);
            
            return (result?.Summary ?? string.Empty, 
                    result?.Contributions ?? Array.Empty<string>(), 
                    result?.Anchors ?? Array.Empty<AnchorDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to summarize paper {PaperId}", paperId);
            throw;
        }
    }

    public async Task<RelatedPayload> RelatedAsync(string? title, string[]? keyphrases, SectionDto[]? sections)
    {
        if (_options.Stub)
        {
            return GenerateStubRelated(title);
        }

        try
        {
            var request = new { title, keyphrases, sections };
            var response = await _httpClient.PostAsJsonAsync("/related", request);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RelatedPayload>(content) ?? new RelatedPayload();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get related works for title: {Title}", title);
            throw;
        }
    }

    private static SectionDto[] GenerateStubSections(Guid paperId)
    {
        return new[]
        {
            new SectionDto
            {
                Name = "Abstract",
                Text = "This paper presents a novel approach to machine learning that demonstrates significant improvements over existing methods.",
                Tokens = 150,
                PageStart = 1,
                PageEnd = 1,
                OrderIdx = 0
            },
            new SectionDto
            {
                Name = "Introduction",
                Text = "Machine learning has become increasingly important in various domains. This work addresses key challenges in the field.",
                Tokens = 300,
                PageStart = 1,
                PageEnd = 2,
                OrderIdx = 1
            },
            new SectionDto
            {
                Name = "Methods",
                Text = "We propose a new algorithm that combines deep learning with traditional statistical methods.",
                Tokens = 500,
                PageStart = 3,
                PageEnd = 5,
                OrderIdx = 2
            },
            new SectionDto
            {
                Name = "Results",
                Text = "Our experiments show a 15% improvement in accuracy compared to baseline methods.",
                Tokens = 400,
                PageStart = 6,
                PageEnd = 8,
                OrderIdx = 3
            },
            new SectionDto
            {
                Name = "Conclusion",
                Text = "This work demonstrates the effectiveness of our approach and opens new avenues for future research.",
                Tokens = 200,
                PageStart = 9,
                PageEnd = 9,
                OrderIdx = 4
            }
        };
    }

    private static (string summary, string[] contributions, AnchorDto[] anchors) GenerateStubSummary(Guid paperId)
    {
        var summary = "This paper introduces a novel machine learning approach that combines deep learning with traditional statistical methods, achieving a 15% improvement in accuracy over existing baseline methods. The work addresses key challenges in the field and demonstrates significant potential for practical applications.";
        
        var contributions = new[]
        {
            "Novel hybrid algorithm combining deep learning and statistical methods",
            "15% improvement in accuracy over baseline approaches",
            "Comprehensive evaluation on multiple benchmark datasets",
            "Open-source implementation made available to the research community"
        };

        var anchors = new[]
        {
            new AnchorDto { BulletIndex = 0, SectionName = "Methods", PageStart = 3, PageEnd = 5 },
            new AnchorDto { BulletIndex = 1, SectionName = "Results", PageStart = 6, PageEnd = 8 },
            new AnchorDto { BulletIndex = 2, SectionName = "Results", PageStart = 7, PageEnd = 8 },
            new AnchorDto { BulletIndex = 3, SectionName = "Conclusion", PageStart = 9, PageEnd = 9 }
        };

        return (summary, contributions, anchors);
    }

    private static RelatedPayload GenerateStubRelated(string? title)
    {
        return new RelatedPayload
        {
            Provider = "OpenAlex",
            Items = new List<RelatedItem>
            {
                new RelatedItem
                {
                    Title = "Deep Learning for Machine Learning: A Comprehensive Survey",
                    Authors = "Smith, J.; Johnson, A.; Brown, M.",
                    Venue = "Journal of Machine Learning Research",
                    Year = 2023,
                    Url = "https://example.com/paper1",
                    Reason = "Similar methodology using deep learning approaches"
                },
                new RelatedItem
                {
                    Title = "Statistical Methods in Modern AI: Bridging Traditional and Contemporary Approaches",
                    Authors = "Davis, K.; Wilson, L.",
                    Venue = "Conference on Neural Information Processing Systems",
                    Year = 2022,
                    Url = "https://example.com/paper2",
                    Reason = "Combines statistical methods with modern AI techniques"
                },
                new RelatedItem
                {
                    Title = "Benchmark Evaluation of Machine Learning Algorithms",
                    Authors = "Garcia, R.; Martinez, S.; Lee, C.",
                    Venue = "International Conference on Machine Learning",
                    Year = 2023,
                    Url = "https://example.com/paper3",
                    Reason = "Comprehensive evaluation methodology on benchmark datasets"
                }
            }
        };
    }

    private class SummarizeResponse
    {
        public string Summary { get; set; } = string.Empty;
        public string[] Contributions { get; set; } = Array.Empty<string>();
        public AnchorDto[] Anchors { get; set; } = Array.Empty<AnchorDto>();
    }
}

public class NlpOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public bool Stub { get; set; } = false;
}
