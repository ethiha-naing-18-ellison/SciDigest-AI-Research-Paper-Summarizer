using System.Text;
using System.Text.RegularExpressions;

namespace Api.Services.DocumentConverter;

public class TexConverter : IDocumentConverter
{
    private readonly ILogger<TexConverter> _logger;

    public TexConverter(ILogger<TexConverter> logger)
    {
        _logger = logger;
    }

    public bool CanConvert(string fileName, string? contentType)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension == ".tex" || 
               contentType == "text/x-tex" || 
               contentType == "application/x-tex" ||
               (extension == ".txt" && fileName.ToLowerInvariant().Contains(".tex"));
    }

    public async Task<string> ConvertToTextAsync(Stream fileStream, string fileName)
    {
        try
        {
            using var reader = new StreamReader(fileStream, Encoding.UTF8);
            var texContent = await reader.ReadToEndAsync();
            
            var text = ProcessTexContent(texContent);
            
            _logger.LogInformation("Successfully converted TEX file '{FileName}' to text ({Length} characters)", 
                fileName, text.Length);
            
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert TEX file '{FileName}'", fileName);
            throw new InvalidOperationException($"Failed to convert TEX file: {ex.Message}", ex);
        }
    }

    private static string ProcessTexContent(string texContent)
    {
        var text = texContent;
        
        // Remove comments (lines starting with %)
        text = Regex.Replace(text, @"^%.*$", "", RegexOptions.Multiline);
        
        // Remove LaTeX commands that don't contribute to content
        text = RemoveLatexCommands(text);
        
        // Process document structure commands
        text = ProcessDocumentStructure(text);
        
        // Process formatting commands
        text = ProcessFormatting(text);
        
        // Process math environments
        text = ProcessMathEnvironments(text);
        
        // Process citations and references
        text = ProcessCitations(text);
        
        // Clean up whitespace
        text = CleanupWhitespace(text);
        
        return text.Trim();
    }

    private static string RemoveLatexCommands(string text)
    {
        // Remove preamble commands
        text = Regex.Replace(text, @"\\documentclass\{[^}]*\}", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\usepackage(\[[^\]]*\])?\{[^}]*\}", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\newcommand\{[^}]*\}\{[^}]*\}", "", RegexOptions.IgnoreCase);
        
        // Remove document environment markers
        text = Regex.Replace(text, @"\\begin\{document\}", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\end\{document\}", "", RegexOptions.IgnoreCase);
        
        // Remove other common commands
        text = Regex.Replace(text, @"\\maketitle", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\tableofcontents", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\newpage", "\n\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\clearpage", "\n\n", RegexOptions.IgnoreCase);
        
        return text;
    }

    private static string ProcessDocumentStructure(string text)
    {
        // Process titles and authors
        text = Regex.Replace(text, @"\\title\{([^}]*)\}", "$1\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\author\{([^}]*)\}", "Author: $1\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\date\{([^}]*)\}", "Date: $1\n", RegexOptions.IgnoreCase);
        
        // Process sectioning
        text = Regex.Replace(text, @"\\chapter\{([^}]*)\}", "\n\nChapter: $1\n\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\section\{([^}]*)\}", "\n\nSection: $1\n\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\subsection\{([^}]*)\}", "\n\nSubsection: $1\n\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\subsubsection\{([^}]*)\}", "\n\nSubsubsection: $1\n\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\paragraph\{([^}]*)\}", "\n\nParagraph: $1\n\n", RegexOptions.IgnoreCase);
        
        return text;
    }

    private static string ProcessFormatting(string text)
    {
        // Process emphasis and bold
        text = Regex.Replace(text, @"\\emph\{([^}]*)\}", "$1", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\textbf\{([^}]*)\}", "$1", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\textit\{([^}]*)\}", "$1", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\texttt\{([^}]*)\}", "$1", RegexOptions.IgnoreCase);
        
        // Process lists
        text = Regex.Replace(text, @"\\begin\{itemize\}", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\end\{itemize\}", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\begin\{enumerate\}", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\end\{enumerate\}", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\item", "• ", RegexOptions.IgnoreCase);
        
        // Process quotes
        text = Regex.Replace(text, @"\\begin\{quote\}", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\end\{quote\}", "\n", RegexOptions.IgnoreCase);
        
        return text;
    }

    private static string ProcessMathEnvironments(string text)
    {
        // Remove inline math
        text = Regex.Replace(text, @"\$([^$]*)\$", "[MATH: $1]");
        
        // Remove display math
        text = Regex.Replace(text, @"\$\$([^$]*)\$\$", "\n[MATH EQUATION]\n");
        
        // Remove math environments
        text = Regex.Replace(text, @"\\begin\{equation\*?\}.*?\\end\{equation\*?\}", "\n[EQUATION]\n", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\begin\{align\*?\}.*?\\end\{align\*?\}", "\n[EQUATIONS]\n", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\begin\{gather\*?\}.*?\\end\{gather\*?\}", "\n[EQUATIONS]\n", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        
        return text;
    }

    private static string ProcessCitations(string text)
    {
        // Process citations
        text = Regex.Replace(text, @"\\cite\{([^}]*)\}", "[CITE: $1]", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\citep\{([^}]*)\}", "[CITE: $1]", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\citet\{([^}]*)\}", "[CITE: $1]", RegexOptions.IgnoreCase);
        
        // Process labels and references
        text = Regex.Replace(text, @"\\label\{([^}]*)\}", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\\ref\{([^}]*)\}", "[REF: $1]", RegexOptions.IgnoreCase);
        
        return text;
    }

    private static string CleanupWhitespace(string text)
    {
        // Remove extra braces
        text = Regex.Replace(text, @"\{([^{}]*)\}", "$1");
        
        // Clean up multiple newlines
        text = Regex.Replace(text, @"\n\s*\n\s*\n", "\n\n");
        
        // Remove leading/trailing whitespace on lines
        var lines = text.Split('\n').Select(line => line.Trim());
        text = string.Join("\n", lines);
        
        // Remove empty lines at start and end
        text = text.Trim();
        
        return text;
    }
}
