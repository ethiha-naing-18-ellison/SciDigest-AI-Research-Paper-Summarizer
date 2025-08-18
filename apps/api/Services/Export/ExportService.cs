using Api.Models;

namespace Api.Services.Export;

public class ExportService : IExporter
{
    private readonly MarkdownExporter _markdownExporter;
    private readonly PdfExporter _pdfExporter;

    public ExportService(MarkdownExporter markdownExporter, PdfExporter pdfExporter)
    {
        _markdownExporter = markdownExporter;
        _pdfExporter = pdfExporter;
    }

    public Task<byte[]> ExportMarkdownAsync(PaperResponse paper)
    {
        var markdown = _markdownExporter.GenerateMarkdown(paper);
        return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(markdown));
    }

    public async Task<byte[]> ExportPdfAsync(PaperResponse paper)
    {
        return await _pdfExporter.GeneratePdfAsync(paper);
    }
}
