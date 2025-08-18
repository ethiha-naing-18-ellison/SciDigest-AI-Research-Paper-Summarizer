using Api.Domain.Entities;
using AutoMapper;
using System.Text.Json;

namespace Api.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Paper, PaperResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Sections, opt => opt.MapFrom(src => src.Sections.OrderBy(s => s.OrderIdx)))
            .ForMember(dest => dest.Summary, opt => opt.MapFrom(src => src.Summaries.OrderByDescending(s => s.CreatedAt).FirstOrDefault()))
            .ForMember(dest => dest.Contributions, opt => opt.MapFrom(src => src.Contributions.OrderByDescending(c => c.CreatedAt).FirstOrDefault()))
            .ForMember(dest => dest.Related, opt => opt.MapFrom(src => CombineAllRelatedWork(src.RelatedWorks)));

        CreateMap<PaperSection, PaperSectionResponse>();

        CreateMap<Summary, SummaryResponse>();

        CreateMap<Contribution, ContributionsResponse>()
            .ForMember(dest => dest.Bullets, opt => opt.MapFrom(src => 
                ParseContributionBullets(src.BulletsJson)))
            .ForMember(dest => dest.Anchors, opt => opt.MapFrom(src => 
                ParseContributionAnchors(src.BulletsJson)));

        CreateMap<RelatedWork, RelatedResponse>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => 
                ParseRelatedItems(src.ResultsJson)));
    }

    private static string[] ParseContributionBullets(string bulletsJson)
    {
        if (string.IsNullOrEmpty(bulletsJson))
            return Array.Empty<string>();
        
        try
        {
            var data = JsonSerializer.Deserialize<ContributionData>(bulletsJson);
            return data?.Bullets ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static AnchorDto[] ParseContributionAnchors(string bulletsJson)
    {
        if (string.IsNullOrEmpty(bulletsJson))
            return Array.Empty<AnchorDto>();
        
        try
        {
            var data = JsonSerializer.Deserialize<ContributionData>(bulletsJson);
            return data?.Anchors ?? Array.Empty<AnchorDto>();
        }
        catch
        {
            return Array.Empty<AnchorDto>();
        }
    }

    private static RelatedItem[] ParseRelatedItems(string resultsJson)
    {
        if (string.IsNullOrEmpty(resultsJson))
            return Array.Empty<RelatedItem>();
        
        try
        {
            return JsonSerializer.Deserialize<RelatedItem[]>(resultsJson) ?? Array.Empty<RelatedItem>();
        }
        catch
        {
            return Array.Empty<RelatedItem>();
        }
    }

    private static RelatedResponse? CombineAllRelatedWork(IEnumerable<RelatedWork> relatedWorks)
    {
        var allRelatedWorks = relatedWorks
            .OrderBy(r => r.Provider == "OpenAlex" ? 0 : 1) // Prefer OpenAlex first
            .ThenByDescending(r => r.CreatedAt)
            .ToList();

        if (!allRelatedWorks.Any())
            return null;

        var allItems = new List<RelatedItem>();
        string primaryProvider = "Combined";

        foreach (var work in allRelatedWorks)
        {
            var items = ParseRelatedItems(work.ResultsJson);
            allItems.AddRange(items);
            
            // Use the first provider as the primary one
            if (primaryProvider == "Combined")
            {
                primaryProvider = work.Provider;
            }
        }

        // If we have multiple providers, indicate that it's combined
        if (allRelatedWorks.Select(w => w.Provider).Distinct().Count() > 1)
        {
            var providers = string.Join(" + ", allRelatedWorks.Select(w => w.Provider).Distinct());
            primaryProvider = providers;
        }

        return new RelatedResponse
        {
            Provider = primaryProvider,
            Items = allItems.ToArray()
        };
    }
}

public class ContributionData
{
    public string[] Bullets { get; set; } = Array.Empty<string>();
    public AnchorDto[]? Anchors { get; set; }
}
