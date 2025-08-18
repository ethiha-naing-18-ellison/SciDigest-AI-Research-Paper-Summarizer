using Api.Models;

namespace Api.Services.Export;

public interface IExporter
{
    Task<byte[]> ExportMarkdownAsync(PaperResponse paper);
    Task<byte[]> ExportPdfAsync(PaperResponse paper);
}
