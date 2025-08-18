using Api.Models;
using PuppeteerSharp;
using System.Text;

namespace Api.Services.Export;

public class PdfExporter
{
    private readonly MarkdownExporter _markdownExporter;
    private readonly ILogger<PdfExporter> _logger;
    private static bool _browserDownloaded = false;
    private static readonly SemaphoreSlim _downloadSemaphore = new(1, 1);

    public PdfExporter(MarkdownExporter markdownExporter, ILogger<PdfExporter> logger)
    {
        _markdownExporter = markdownExporter;
        _logger = logger;
    }

    public async Task<byte[]> GeneratePdfAsync(PaperResponse paper)
    {
        await EnsureBrowserDownloadedAsync();
        
        var markdown = _markdownExporter.GenerateMarkdown(paper);
        var html = ConvertMarkdownToHtml(markdown);
        
        var launchOptions = new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        };

        await using var browser = await Puppeteer.LaunchAsync(launchOptions);
        await using var page = await browser.NewPageAsync();
        
        await page.SetContentAsync(html);
        
        var pdfOptions = new PdfOptions
        {
            Format = PuppeteerSharp.Media.PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new PuppeteerSharp.Media.MarginOptions
            {
                Top = "1cm",
                Right = "1cm",
                Bottom = "1cm",
                Left = "1cm"
            }
        };
        
        var pdfBytes = await page.PdfDataAsync(pdfOptions);
        return pdfBytes;
    }

    private async Task EnsureBrowserDownloadedAsync()
    {
        if (_browserDownloaded) return;

        await _downloadSemaphore.WaitAsync();
        try
        {
            if (_browserDownloaded) return;

            _logger.LogInformation("Downloading Chromium browser for PDF generation...");
            
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            
            _browserDownloaded = true;
            _logger.LogInformation("Chromium browser downloaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download Chromium browser");
            throw;
        }
        finally
        {
            _downloadSemaphore.Release();
        }
    }

    private static string ConvertMarkdownToHtml(string markdown)
    {
        var html = new StringBuilder();
        
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset=\"utf-8\">");
        html.AppendLine("<title>Research Paper Summary</title>");
        html.AppendLine("<style>");
        html.AppendLine(GetCssStyles());
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        // Simple markdown to HTML conversion
        var lines = markdown.Split('\n');
        var inCodeBlock = false;
        
        foreach (var line in lines)
        {
            if (line.StartsWith("```"))
            {
                inCodeBlock = !inCodeBlock;
                html.AppendLine(inCodeBlock ? "<pre><code>" : "</code></pre>");
                continue;
            }
            
            if (inCodeBlock)
            {
                html.AppendLine(System.Web.HttpUtility.HtmlEncode(line));
                continue;
            }
            
            if (line.StartsWith("# "))
            {
                html.AppendLine($"<h1>{System.Web.HttpUtility.HtmlEncode(line[2..])}</h1>");
            }
            else if (line.StartsWith("## "))
            {
                html.AppendLine($"<h2>{System.Web.HttpUtility.HtmlEncode(line[3..])}</h2>");
            }
            else if (line.StartsWith("### "))
            {
                html.AppendLine($"<h3>{System.Web.HttpUtility.HtmlEncode(line[4..])}</h3>");
            }
            else if (line.StartsWith("**") && line.EndsWith("**") && line.Length > 4)
            {
                var content = line[2..^2];
                html.AppendLine($"<p><strong>{System.Web.HttpUtility.HtmlEncode(content)}</strong></p>");
            }
            else if (line.StartsWith("- "))
            {
                var content = line[2..];
                
                // Handle markdown links
                content = ConvertMarkdownLinks(content);
                
                html.AppendLine($"<li>{content}</li>");
            }
            else if (line.StartsWith("  *") && line.EndsWith("*"))
            {
                var content = line[3..^1];
                html.AppendLine($"<p class=\"reference\"><em>{System.Web.HttpUtility.HtmlEncode(content)}</em></p>");
            }
            else if (line.StartsWith("*") && line.EndsWith("*") && line.Length > 2)
            {
                var content = line[1..^1];
                html.AppendLine($"<p class=\"italic\">{System.Web.HttpUtility.HtmlEncode(content)}</p>");
            }
            else if (line.StartsWith("---"))
            {
                html.AppendLine("<hr>");
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                var content = ConvertMarkdownLinks(line);
                html.AppendLine($"<p>{content}</p>");
            }
            else
            {
                html.AppendLine("<br>");
            }
        }
        
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        return html.ToString();
    }

    private static string ConvertMarkdownLinks(string text)
    {
        // Convert [text](url) to <a href="url">text</a>
        var linkPattern = @"\[([^\]]+)\]\(([^)]+)\)";
        return System.Text.RegularExpressions.Regex.Replace(text, linkPattern, 
            match => $"<a href=\"{System.Web.HttpUtility.HtmlEncode(match.Groups[2].Value)}\">{System.Web.HttpUtility.HtmlEncode(match.Groups[1].Value)}</a>");
    }

    private static string GetCssStyles()
    {
        return @"
body {
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    line-height: 1.6;
    margin: 0;
    padding: 20px;
    color: #333;
}

h1 {
    color: #2c3e50;
    border-bottom: 3px solid #3498db;
    padding-bottom: 10px;
    margin-bottom: 20px;
}

h2 {
    color: #34495e;
    border-bottom: 2px solid #bdc3c7;
    padding-bottom: 5px;
    margin-top: 30px;
    margin-bottom: 15px;
}

h3 {
    color: #7f8c8d;
    margin-top: 20px;
    margin-bottom: 10px;
}

p {
    margin-bottom: 10px;
    text-align: justify;
}

ul, ol {
    padding-left: 20px;
}

li {
    margin-bottom: 5px;
    list-style-type: disc;
}

.reference {
    font-size: 0.9em;
    color: #7f8c8d;
    margin-left: 20px;
    margin-top: 0;
}

.italic {
    font-style: italic;
    color: #7f8c8d;
    text-align: center;
}

hr {
    border: none;
    height: 1px;
    background-color: #bdc3c7;
    margin: 20px 0;
}

a {
    color: #3498db;
    text-decoration: none;
}

a:hover {
    text-decoration: underline;
}

strong {
    color: #2c3e50;
}

pre, code {
    background-color: #f8f9fa;
    border: 1px solid #e9ecef;
    border-radius: 3px;
    padding: 10px;
    font-family: 'Courier New', monospace;
    font-size: 0.9em;
}

@media print {
    body {
        margin: 0;
        padding: 15px;
    }
    
    h1, h2, h3 {
        page-break-after: avoid;
    }
    
    ul, ol {
        page-break-inside: avoid;
    }
}";
    }
}
