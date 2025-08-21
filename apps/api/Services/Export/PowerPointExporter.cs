using Api.Models;
using System.Text;

namespace Api.Services.Export;

public class PowerPointExporter
{
    private readonly ILogger<PowerPointExporter> _logger;

    public PowerPointExporter(ILogger<PowerPointExporter> logger)
    {
        _logger = logger;
    }

    public Task<byte[]> GeneratePowerPointAsync(PaperResponse paper)
    {
        try
        {
            // For now, we'll create a simple PowerPoint-like structure using XML
            // In a production environment, you might want to use a library like DocumentFormat.OpenXml
            
            var pptxContent = GeneratePptxContent(paper);
            return Task.FromResult(Encoding.UTF8.GetBytes(pptxContent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PowerPoint for paper {PaperId}", paper.Id);
            throw;
        }
    }

    private string GeneratePptxContent(PaperResponse paper)
    {
        var sb = new StringBuilder();
        
        // Create a simple HTML-based presentation that can be converted to PowerPoint
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='UTF-8'>");
        sb.AppendLine("<title>Research Paper Summary</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 0; padding: 20px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); }");
        sb.AppendLine(".slide { background: white; margin: 20px auto; padding: 40px; border-radius: 10px; box-shadow: 0 10px 30px rgba(0,0,0,0.3); max-width: 800px; }");
        sb.AppendLine(".title { color: #2c3e50; font-size: 28px; font-weight: bold; margin-bottom: 20px; text-align: center; }");
        sb.AppendLine(".subtitle { color: #7f8c8d; font-size: 18px; margin-bottom: 30px; text-align: center; }");
        sb.AppendLine(".section-title { color: #34495e; font-size: 24px; font-weight: bold; margin: 20px 0 10px 0; border-bottom: 2px solid #3498db; padding-bottom: 5px; }");
        sb.AppendLine(".content { color: #2c3e50; font-size: 16px; line-height: 1.6; margin: 10px 0; }");
        sb.AppendLine(".bullet { margin: 10px 0; padding-left: 20px; }");
        sb.AppendLine(".bullet:before { content: '• '; color: #3498db; font-weight: bold; }");
        sb.AppendLine(".highlight { background: #f39c12; color: white; padding: 2px 6px; border-radius: 3px; }");
        sb.AppendLine(".tech-tag { background: #3498db; color: white; padding: 4px 8px; border-radius: 15px; font-size: 12px; margin: 2px; display: inline-block; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // Title Slide
        sb.AppendLine("<div class='slide'>");
        sb.AppendLine($"<div class='title'>{paper.Title ?? "Research Paper Summary"}</div>");
        sb.AppendLine($"<div class='subtitle'>{paper.Authors ?? "Unknown Authors"}</div>");
        if (paper.Year.HasValue || !string.IsNullOrEmpty(paper.Venue))
        {
            sb.AppendLine($"<div class='subtitle'>{paper.Venue} • {paper.Year}</div>");
        }
        sb.AppendLine("</div>");

        // Executive Summary Slide
        if (paper.Summary?.ExecutiveSummary != null)
        {
            sb.AppendLine("<div class='slide'>");
            sb.AppendLine("<div class='section-title'>Executive Summary</div>");
            sb.AppendLine($"<div class='content'>{paper.Summary.ExecutiveSummary}</div>");
            sb.AppendLine("</div>");
        }

        // Key Contributions Slide
        if (paper.Contributions?.Bullets != null && paper.Contributions.Bullets.Length > 0)
        {
            sb.AppendLine("<div class='slide'>");
            sb.AppendLine("<div class='section-title'>Key Contributions</div>");
            foreach (var contribution in paper.Contributions.Bullets.Take(5)) // Limit to 5 for presentation
            {
                sb.AppendLine($"<div class='bullet'>{contribution}</div>");
            }
            sb.AppendLine("</div>");
        }

        // Technical Details Slide
        if (paper.Summary?.TechnicalDetails != null)
        {
            sb.AppendLine("<div class='slide'>");
            sb.AppendLine("<div class='section-title'>Technical Details</div>");
            sb.AppendLine($"<div class='content'>{paper.Summary.TechnicalDetails}</div>");
            sb.AppendLine("</div>");
        }

        // Methodology Slide
        if (paper.Summary?.Methodology != null)
        {
            sb.AppendLine("<div class='slide'>");
            sb.AppendLine("<div class='section-title'>Methodology</div>");
            sb.AppendLine($"<div class='content'>{paper.Summary.Methodology}</div>");
            sb.AppendLine("</div>");
        }

        // Results Slide
        if (paper.Summary?.Results != null)
        {
            sb.AppendLine("<div class='slide'>");
            sb.AppendLine("<div class='section-title'>Results & Findings</div>");
            sb.AppendLine($"<div class='content'>{paper.Summary.Results}</div>");
            sb.AppendLine("</div>");
        }

        // Impact Slide
        if (paper.Summary?.Impact != null)
        {
            sb.AppendLine("<div class='slide'>");
            sb.AppendLine("<div class='section-title'>Impact & Significance</div>");
            sb.AppendLine($"<div class='content'>{paper.Summary.Impact}</div>");
            sb.AppendLine("</div>");
        }

        // Related Work Slide
        if (paper.Related?.Items != null && paper.Related.Items.Length > 0)
        {
            sb.AppendLine("<div class='slide'>");
            sb.AppendLine("<div class='section-title'>Related Work</div>");
            foreach (var item in paper.Related.Items.Take(3)) // Limit to 3 for presentation
            {
                sb.AppendLine($"<div class='bullet'><strong>{item.Title}</strong> - {item.Authors}</div>");
                sb.AppendLine($"<div class='content' style='margin-left: 20px; font-size: 14px; color: #7f8c8d;'>{item.Reason}</div>");
            }
            sb.AppendLine("</div>");
        }

        // Thank You Slide
        sb.AppendLine("<div class='slide'>");
        sb.AppendLine("<div class='section-title' style='text-align: center; margin-top: 100px;'>Thank You</div>");
        sb.AppendLine("<div class='content' style='text-align: center; margin-top: 50px;'>");
        sb.AppendLine("Generated by SciDigest AI Research Paper Summarizer<br>");
        sb.AppendLine("<span class='tech-tag'>AI-Powered</span> <span class='tech-tag'>Research</span> <span class='tech-tag'>Analysis</span>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }
}
