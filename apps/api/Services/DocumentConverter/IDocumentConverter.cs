namespace Api.Services.DocumentConverter;

public interface IDocumentConverter
{
    Task<string> ConvertToTextAsync(Stream fileStream, string fileName);
    bool CanConvert(string fileName, string? contentType);
}

public class DocumentFormat
{
    public const string PDF = "pdf";
    public const string DOCX = "docx";
    public const string DOC = "doc";
    public const string HTML = "html";
    public const string TEX = "tex";
}

public class ConvertedDocument
{
    public string Text { get; set; } = string.Empty;
    public string OriginalFormat { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}
