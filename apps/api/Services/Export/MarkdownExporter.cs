using Api.Models;
using System.Text;

namespace Api.Services.Export;

public class MarkdownExporter
{
    public string GenerateMarkdown(PaperResponse paper)
    {
        var sb = new StringBuilder();
        
        // Title
        if (!string.IsNullOrEmpty(paper.Title))
        {
            sb.AppendLine($"# {paper.Title}");
            sb.AppendLine();
        }
        
        // Authors
        if (!string.IsNullOrEmpty(paper.Authors))
        {
            sb.AppendLine($"**Authors:** {paper.Authors}");
            sb.AppendLine();
        }
        
        // Year and Venue
        if (paper.Year.HasValue || !string.IsNullOrEmpty(paper.Venue))
        {
            var yearVenue = new List<string>();
            if (paper.Year.HasValue) yearVenue.Add(paper.Year.Value.ToString());
            if (!string.IsNullOrEmpty(paper.Venue)) yearVenue.Add(paper.Venue);
            
            sb.AppendLine($"**Published:** {string.Join(", ", yearVenue)}");
            sb.AppendLine();
        }
        
        // Pages
        sb.AppendLine($"**Pages:** {paper.Pages}");
        sb.AppendLine();
        
        // Status
        sb.AppendLine($"**Processing Status:** {paper.Status}");
        sb.AppendLine();
        
        // Executive Summary
        if (paper.Summary != null && !string.IsNullOrEmpty(paper.Summary.ExecutiveSummary))
        {
            sb.AppendLine("## Executive Summary");
            sb.AppendLine();
            sb.AppendLine(paper.Summary.ExecutiveSummary);
            sb.AppendLine();
        }
        
        // Key Contributions
        if (paper.Contributions != null && paper.Contributions.Bullets.Length > 0)
        {
            sb.AppendLine("## Key Contributions");
            sb.AppendLine();
            
            for (int i = 0; i < paper.Contributions.Bullets.Length; i++)
            {
                var bullet = paper.Contributions.Bullets[i];
                sb.AppendLine($"- {bullet}");
                
                // Add anchor information if available
                var anchor = paper.Contributions.Anchors?.FirstOrDefault(a => a.BulletIndex == i);
                if (anchor != null)
                {
                    sb.AppendLine($"  *Reference: {anchor.SectionName}, Pages {anchor.PageStart}-{anchor.PageEnd}*");
                }
            }
            sb.AppendLine();
        }
        
        // Related Work
        if (paper.Related != null && paper.Related.Items.Length > 0)
        {
            sb.AppendLine("## Related Work");
            sb.AppendLine();
            sb.AppendLine($"*Source: {paper.Related.Provider}*");
            sb.AppendLine();
            
            foreach (var item in paper.Related.Items)
            {
                sb.AppendLine($"### {item.Title}");
                sb.AppendLine();
                sb.AppendLine($"**Authors:** {item.Authors}");
                
                if (item.Year.HasValue || !string.IsNullOrEmpty(item.Venue))
                {
                    var yearVenue = new List<string>();
                    if (item.Year.HasValue) yearVenue.Add(item.Year.Value.ToString());
                    if (!string.IsNullOrEmpty(item.Venue)) yearVenue.Add(item.Venue);
                    
                    sb.AppendLine($"**Published:** {string.Join(", ", yearVenue)}");
                }
                
                if (!string.IsNullOrEmpty(item.Url))
                {
                    sb.AppendLine($"**URL:** [{item.Url}]({item.Url})");
                }
                
                if (!string.IsNullOrEmpty(item.Reason))
                {
                    sb.AppendLine($"**Relevance:** {item.Reason}");
                }
                
                sb.AppendLine();
            }
        }
        
        // Paper Sections Overview
        if (paper.Sections != null && paper.Sections.Length > 0)
        {
            sb.AppendLine("## Paper Structure");
            sb.AppendLine();
            
            foreach (var section in paper.Sections)
            {
                sb.AppendLine($"- **{section.Name}** (Pages {section.PageStart}-{section.PageEnd}, {section.Tokens:N0} tokens)");
            }
            sb.AppendLine();
        }
        
        // Footer
        sb.AppendLine("---");
        sb.AppendLine($"*Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC*");
        
        return sb.ToString();
    }
}
