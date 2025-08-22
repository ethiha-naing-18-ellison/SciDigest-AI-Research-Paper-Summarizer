using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace Api.Services.DocumentConverter;

public class DocxConverter : IDocumentConverter
{
    private readonly ILogger<DocxConverter> _logger;

    public DocxConverter(ILogger<DocxConverter> logger)
    {
        _logger = logger;
    }

    public bool CanConvert(string fileName, string? contentType)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension == ".docx" || 
               contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    }

    public Task<string> ConvertToTextAsync(Stream fileStream, string fileName)
    {
        try
        {
            using var wordDocument = WordprocessingDocument.Open(fileStream, false);
            var body = wordDocument.MainDocumentPart?.Document?.Body;
            
            if (body == null)
            {
                throw new InvalidOperationException("Document body not found");
            }

            var text = new StringBuilder();
            
            // Extract text from paragraphs
            foreach (var paragraph in body.Elements<Paragraph>())
            {
                var paragraphText = ExtractTextFromParagraph(paragraph);
                if (!string.IsNullOrWhiteSpace(paragraphText))
                {
                    text.AppendLine(paragraphText);
                }
            }
            
            // Extract text from tables
            foreach (var table in body.Elements<Table>())
            {
                var tableText = ExtractTextFromTable(table);
                if (!string.IsNullOrWhiteSpace(tableText))
                {
                    text.AppendLine(tableText);
                }
            }

            var result = text.ToString().Trim();
            _logger.LogInformation("Successfully converted DOCX file '{FileName}' to text ({Length} characters)", 
                fileName, result.Length);
            
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert DOCX file '{FileName}'", fileName);
            throw new InvalidOperationException($"Failed to convert DOCX file: {ex.Message}", ex);
        }
    }

    private static string ExtractTextFromParagraph(Paragraph paragraph)
    {
        var text = new StringBuilder();
        
        foreach (var run in paragraph.Elements<Run>())
        {
            foreach (var textElement in run.Elements<Text>())
            {
                text.Append(textElement.Text);
            }
        }
        
        return text.ToString();
    }

    private static string ExtractTextFromTable(Table table)
    {
        var text = new StringBuilder();
        
        foreach (var row in table.Elements<TableRow>())
        {
            var rowText = new StringBuilder();
            
            foreach (var cell in row.Elements<TableCell>())
            {
                var cellText = new StringBuilder();
                
                foreach (var paragraph in cell.Elements<Paragraph>())
                {
                    var paragraphText = ExtractTextFromParagraph(paragraph);
                    if (!string.IsNullOrWhiteSpace(paragraphText))
                    {
                        cellText.Append(paragraphText + " ");
                    }
                }
                
                if (cellText.Length > 0)
                {
                    rowText.Append(cellText.ToString().Trim() + "\t");
                }
            }
            
            if (rowText.Length > 0)
            {
                text.AppendLine(rowText.ToString().TrimEnd('\t'));
            }
        }
        
        return text.ToString();
    }
}
