using Api.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Net.Http.Json;

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

    public async Task<ParseResultDto> ParseAsync(Guid paperId, string filePath)
    {
        if (_options.Stub)
        {
            var sections = GenerateStubSections(paperId);
            return new ParseResultDto(null, sections);
        }

        try
        {
            var payload = new { paper_id = paperId.ToString(), file_path = filePath };
            var resp = await _httpClient.PostAsJsonAsync("parse", payload);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadFromJsonAsync<ParseResultDto>()
                       ?? throw new InvalidOperationException("Empty /parse body");
            return body;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse paper {PaperId}", paperId);
            throw;
        }
    }

    public async Task<(string summary, string[] contributions, string[]? details, AnchorDto[] anchors, string abstract_text, string introduction, string methodology, string results, string discussion, string limitations, string technicalDetails, string impact)> SummarizeAsync(Guid paperId, SectionDto[] sections)
    {
        if (_options.Stub)
        {
            return GenerateStubSummary(paperId);
        }

        try
        {
            var payload = new { paper_id = paperId.ToString(), sections };
            var resp = await _httpClient.PostAsJsonAsync("summarize", payload);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadFromJsonAsync<SummarizeResponse>()
                       ?? throw new InvalidOperationException("Empty /summarize body");
            
            return (body.Summary ?? string.Empty, 
                    body.Contributions ?? Array.Empty<string>(),
                    body.Details,
                    body.Anchors ?? Array.Empty<AnchorDto>(),
                    body.Abstract ?? string.Empty,
                    body.Introduction ?? string.Empty,
                    body.Methodology ?? string.Empty,
                    body.Results ?? string.Empty,
                    body.Discussion ?? string.Empty,
                    body.Limitations ?? string.Empty,
                    body.TechnicalDetails ?? string.Empty,
                    body.Impact ?? string.Empty);
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
            var payload = new { title, keyphrases, sections };
            var resp = await _httpClient.PostAsJsonAsync("related", payload);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadFromJsonAsync<RelatedPayload>()
                       ?? throw new InvalidOperationException("Empty /related body");
            return body;
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

    private static (string summary, string[] contributions, string[]? details, AnchorDto[] anchors, string abstract_text, string introduction, string methodology, string results, string discussion, string limitations, string technicalDetails, string impact) GenerateStubSummary(Guid paperId)
    {
        var summary = "This research paper presents a comprehensive analysis of machine learning methodologies and their applications in modern artificial intelligence systems. The study investigates novel approaches that combine deep learning techniques with traditional statistical methods, demonstrating significant improvements in accuracy and performance across multiple benchmark datasets. Through extensive experimentation and evaluation, the research establishes new benchmarks for hybrid machine learning approaches while providing valuable insights into the practical implementation of these methodologies in real-world applications.";
        
        var contributions = new[]
        {
            "Novel hybrid algorithm combining deep learning and statistical methods",
            "15% improvement in accuracy over baseline approaches",
            "Comprehensive evaluation on multiple benchmark datasets",
            "Open-source implementation made available to the research community",
            "Establishes new benchmarks for hybrid machine learning approaches",
            "Develops efficient algorithms for real-time processing applications",
            "Analyzes the impact of various hyperparameters on model performance",
            "Compares results with multiple state-of-the-art baseline methods",
            "Provides detailed ablation studies for component analysis",
            "Validates approach through extensive cross-dataset evaluation"
        };

        var anchors = new[]
        {
            new AnchorDto { BulletIndex = 0, SectionName = "Methods", PageStart = 3, PageEnd = 5 },
            new AnchorDto { BulletIndex = 1, SectionName = "Results", PageStart = 6, PageEnd = 8 },
            new AnchorDto { BulletIndex = 2, SectionName = "Results", PageStart = 7, PageEnd = 8 },
            new AnchorDto { BulletIndex = 3, SectionName = "Conclusion", PageStart = 9, PageEnd = 9 },
            new AnchorDto { BulletIndex = 4, SectionName = "Results", PageStart = 6, PageEnd = 8 },
            new AnchorDto { BulletIndex = 5, SectionName = "Methods", PageStart = 3, PageEnd = 5 },
            new AnchorDto { BulletIndex = 6, SectionName = "Results", PageStart = 6, PageEnd = 8 },
            new AnchorDto { BulletIndex = 7, SectionName = "Results", PageStart = 7, PageEnd = 8 },
            new AnchorDto { BulletIndex = 8, SectionName = "Results", PageStart = 6, PageEnd = 8 },
            new AnchorDto { BulletIndex = 9, SectionName = "Results", PageStart = 7, PageEnd = 8 }
        };

        var details = new[]
        {
            "This novel approach introduces a hybrid architecture that effectively combines the representation learning capabilities of deep neural networks with the interpretability and robustness of traditional statistical methods. The algorithm demonstrates superior performance through its ability to leverage both paradigms.",
            "Through comprehensive evaluation across five benchmark datasets, our method consistently achieves 15% higher accuracy compared to state-of-the-art baselines including Random Forest, SVM, and deep learning models. The improvement is statistically significant with p < 0.001.",
            "The evaluation framework includes rigorous testing on MNIST, CIFAR-10, ImageNet, Reuters, and Amazon reviews datasets, providing comprehensive coverage across different domains. Each dataset was split using standard protocols to ensure fair comparison with existing methods.",
            "All implementation code, trained models, and experimental configurations have been made publicly available on GitHub under MIT license. The repository includes detailed documentation, tutorials, and reproducible experiment scripts for community use.",
            "The proposed hybrid approach establishes new performance benchmarks for several key tasks in computer vision and natural language processing. These benchmarks provide valuable reference points for future research in hybrid machine learning methodologies.",
            "The algorithm architecture incorporates novel optimization techniques that enable real-time inference with sub-millisecond latency on standard hardware. This efficiency makes the approach suitable for production deployment in time-critical applications.",
            "A comprehensive hyperparameter analysis reveals the sensitivity of model performance to key architectural choices including layer depth, learning rate schedules, and regularization parameters. The analysis provides practical guidance for model tuning in different domains.",
            "Extensive comparison with 12 state-of-the-art methods including BERT, ResNet, and Transformer variants demonstrates the superiority of our approach. The comparison uses standardized evaluation protocols and identical hardware configurations for fair assessment.",
            "Detailed ablation studies systematically evaluate the contribution of each component including the statistical inference module, deep feature extractor, and hybrid fusion mechanism. Each component contributes significantly to overall performance with complementary strengths.",
            "Cross-dataset validation across multiple domains demonstrates the generalizability of our approach beyond the training distribution. The method maintains robust performance when applied to datasets with different characteristics and domains."
        };

        return (summary, contributions, details, anchors, 
                "This research paper presents a comprehensive analysis of machine learning methodologies and their applications in modern artificial intelligence systems.", 
                "The research addresses key challenges in modern AI systems and explores innovative solutions to complex problems.", 
                "Novel hybrid approach combining multiple techniques and methodologies for enhanced performance.", 
                "Significant improvements in accuracy and efficiency demonstrated across multiple benchmark datasets.", 
                "Results show promising applications in various domains with practical implications for real-world implementation.", 
                "Current limitations include computational complexity and resource requirements for large-scale deployment.", 
                "Advanced algorithms and deep learning architectures implemented with detailed technical specifications.", 
                "Potential to revolutionize machine learning applications and establish new industry standards.");
    }

    private static RelatedPayload GenerateStubRelated(string? title)
    {
        return new RelatedPayload
        {
            Provider = "OpenAlex+SemanticScholar",
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
                },
                new RelatedItem
                {
                    Title = "Hybrid Machine Learning Approaches: A Systematic Review",
                    Authors = "Anderson, P.; Taylor, R.; White, S.",
                    Venue = "IEEE Transactions on Pattern Analysis and Machine Intelligence",
                    Year = 2023,
                    Url = "https://example.com/paper4",
                    Reason = "Directly related to hybrid machine learning methodologies"
                },
                new RelatedItem
                {
                    Title = "Real-time Processing in Machine Learning Systems",
                    Authors = "Miller, J.; Clark, K.; Rodriguez, M.",
                    Venue = "ACM Computing Surveys",
                    Year = 2022,
                    Url = "https://example.com/paper5",
                    Reason = "Addresses real-time processing challenges in ML systems"
                },
                new RelatedItem
                {
                    Title = "Hyperparameter Optimization in Deep Learning",
                    Authors = "Lee, H.; Kim, S.; Park, J.",
                    Venue = "Neural Networks",
                    Year = 2023,
                    Url = "https://example.com/paper6",
                    Reason = "Focuses on hyperparameter analysis and optimization"
                },
                new RelatedItem
                {
                    Title = "State-of-the-Art Comparison in Machine Learning",
                    Authors = "Wang, L.; Chen, X.; Liu, Y.",
                    Venue = "Pattern Recognition",
                    Year = 2023,
                    Url = "https://example.com/paper7",
                    Reason = "Comprehensive comparison with state-of-the-art methods"
                },
                new RelatedItem
                {
                    Title = "Ablation Studies in Deep Learning Research",
                    Authors = "Johnson, M.; Davis, R.; Wilson, T.",
                    Venue = "Computer Vision and Image Understanding",
                    Year = 2022,
                    Url = "https://example.com/paper8",
                    Reason = "Methodology for component analysis and ablation studies"
                },
                new RelatedItem
                {
                    Title = "Cross-Dataset Evaluation in Machine Learning",
                    Authors = "Brown, A.; Green, B.; Black, C.",
                    Venue = "Machine Learning",
                    Year = 2023,
                    Url = "https://example.com/paper9",
                    Reason = "Validation through cross-dataset evaluation approaches"
                },
                new RelatedItem
                {
                    Title = "Computational Efficiency in Neural Networks",
                    Authors = "Garcia, M.; Lopez, N.; Perez, O.",
                    Venue = "Journal of Artificial Intelligence Research",
                    Year = 2022,
                    Url = "https://example.com/paper10",
                    Reason = "Focuses on computational efficiency in neural networks"
                }
            }
        };
    }

    private class SummarizeResponse
    {
        public string Summary { get; set; } = string.Empty;
        public string[] Contributions { get; set; } = Array.Empty<string>();
        public string[]? Details { get; set; }
        public AnchorDto[] Anchors { get; set; } = Array.Empty<AnchorDto>();
        // New comprehensive sections
        public string Abstract { get; set; } = string.Empty;
        public string Introduction { get; set; } = string.Empty;
        public string Methodology { get; set; } = string.Empty;
        public string Results { get; set; } = string.Empty;
        public string Discussion { get; set; } = string.Empty;
        public string Limitations { get; set; } = string.Empty;
        public string TechnicalDetails { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
    }
}

public class NlpOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public bool Stub { get; set; } = false;
}
