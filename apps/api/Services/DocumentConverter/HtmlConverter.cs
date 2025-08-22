using HtmlAgilityPack;
using System.Text;

namespace Api.Services.DocumentConverter;

public class HtmlConverter : IDocumentConverter
{
    private readonly ILogger<HtmlConverter> _logger;

    public HtmlConverter(ILogger<HtmlConverter> logger)
    {
        _logger = logger;
    }

    public bool CanConvert(string fileName, string? contentType)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension == ".html" || extension == ".htm" || extension == ".xhtml" ||
               contentType == "text/html" || contentType == "application/xhtml+xml";
    }

    public async Task<string> ConvertToTextAsync(Stream fileStream, string fileName)
    {
        try
        {
            using var reader = new StreamReader(fileStream, Encoding.UTF8);
            var htmlContent = await reader.ReadToEndAsync();
            
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            
            var text = new StringBuilder();
            
            // Remove script and style elements
            var scriptsAndStyles = doc.DocumentNode.SelectNodes("//script | //style");
            if (scriptsAndStyles != null)
            {
                foreach (var node in scriptsAndStyles)
                {
                    node.Remove();
                }
            }
            
            // Extract text content
            var textContent = ExtractTextFromNode(doc.DocumentNode);
            
            // Clean up the text
            var lines = textContent.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line));
            
            var result = string.Join("\n", lines);
            _logger.LogInformation("Successfully converted HTML file '{FileName}' to text ({Length} characters)", 
                fileName, result.Length);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert HTML file '{FileName}'", fileName);
            throw new InvalidOperationException($"Failed to convert HTML file: {ex.Message}", ex);
        }
    }

    private static string ExtractTextFromNode(HtmlNode node)
    {
        var text = new StringBuilder();
        
        foreach (var child in node.ChildNodes)
        {
            if (child.NodeType == HtmlNodeType.Text)
            {
                var textContent = HtmlEntity.DeEntitize(child.InnerText);
                if (!string.IsNullOrWhiteSpace(textContent))
                {
                    text.Append(textContent);
                }
            }
            else if (child.NodeType == HtmlNodeType.Element)
            {
                // Add line breaks for block elements
                var isBlockElement = IsBlockElement(child.Name);
                
                if (isBlockElement && text.Length > 0 && !text.ToString().EndsWith("\n"))
                {
                    text.AppendLine();
                }
                
                var childText = ExtractTextFromNode(child);
                text.Append(childText);
                
                if (isBlockElement && !string.IsNullOrWhiteSpace(childText))
                {
                    text.AppendLine();
                }
            }
        }
        
        return text.ToString();
    }

    private static bool IsBlockElement(string tagName)
    {
        var blockElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "div", "p", "h1", "h2", "h3", "h4", "h5", "h6", "ul", "ol", "li", 
            "blockquote", "pre", "table", "tr", "td", "th", "article", "section", 
            "header", "footer", "main", "aside", "nav", "figure", "figcaption"
        };
        
        return blockElements.Contains(tagName);
    }
}
