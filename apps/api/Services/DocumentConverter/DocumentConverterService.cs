namespace Api.Services.DocumentConverter;

public interface IDocumentConverterService
{
    Task<ConvertedDocument> ConvertAsync(Stream fileStream, string fileName, string? contentType);
    bool CanConvert(string fileName, string? contentType);
    string GetFileFormat(string fileName, string? contentType);
}

public class DocumentConverterService : IDocumentConverterService
{
    private readonly IEnumerable<IDocumentConverter> _converters;
    private readonly ILogger<DocumentConverterService> _logger;

    public DocumentConverterService(
        IEnumerable<IDocumentConverter> converters,
        ILogger<DocumentConverterService> logger)
    {
        _converters = converters;
        _logger = logger;
    }

    public bool CanConvert(string fileName, string? contentType)
    {
        return _converters.Any(c => c.CanConvert(fileName, contentType));
    }

    public string GetFileFormat(string fileName, string? contentType)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        
        return extension switch
        {
            ".pdf" => DocumentFormat.PDF,
            ".docx" => DocumentFormat.DOCX,
            ".doc" => DocumentFormat.DOC,
            ".html" or ".htm" or ".xhtml" => DocumentFormat.HTML,
            ".tex" => DocumentFormat.TEX,
            _ when contentType == "application/pdf" => DocumentFormat.PDF,
            _ when contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => DocumentFormat.DOCX,
            _ when contentType == "application/msword" => DocumentFormat.DOC,
            _ when contentType == "text/html" || contentType == "application/xhtml+xml" => DocumentFormat.HTML,
            _ when contentType == "text/x-tex" || contentType == "application/x-tex" => DocumentFormat.TEX,
            _ => "unknown"
        };
    }

    public async Task<ConvertedDocument> ConvertAsync(Stream fileStream, string fileName, string? contentType)
    {
        var converter = _converters.FirstOrDefault(c => c.CanConvert(fileName, contentType));
        
        if (converter == null)
        {
            throw new NotSupportedException($"No converter available for file type: {fileName} ({contentType})");
        }

        try
        {
            // Reset stream position
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            var text = await converter.ConvertToTextAsync(fileStream, fileName);
            var format = GetFileFormat(fileName, contentType);

            var result = new ConvertedDocument
            {
                Text = text,
                OriginalFormat = format,
                Metadata = new Dictionary<string, object>
                {
                    ["FileName"] = fileName,
                    ["ContentType"] = contentType ?? "unknown",
                    ["ConvertedAt"] = DateTime.UtcNow,
                    ["TextLength"] = text.Length,
                    ["ConverterType"] = converter.GetType().Name
                }
            };

            _logger.LogInformation("Successfully converted {FileName} ({Format}) to text using {Converter}", 
                fileName, format, converter.GetType().Name);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert document {FileName} ({ContentType})", fileName, contentType);
            throw;
        }
    }
}
