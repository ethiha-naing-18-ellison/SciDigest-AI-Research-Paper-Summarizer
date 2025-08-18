namespace Api.Models;

public class UploadResponse
{
    public Guid PaperId { get; set; }
}

public class ExportRequest
{
    public string Format { get; set; } = string.Empty; // "md" or "pdf"
}
