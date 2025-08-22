using FluentValidation;
using Microsoft.Extensions.Options;

namespace Api.Validation;

public class UploadRequest
{
    public IFormFile File { get; set; } = null!;
}

public class UploadValidator : AbstractValidator<UploadRequest>
{
    private static readonly string[] AllowedContentTypes = {
        "application/pdf",                                                      // PDF
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // DOCX
        "application/msword",                                                   // DOC
        "text/html",                                                           // HTML
        "application/xhtml+xml",                                               // XHTML
        "text/x-tex",                                                          // TEX
        "application/x-tex",                                                   // TEX (alternative)
        "text/plain"                                                           // Plain text (for .tex files)
    };

    private static readonly string[] AllowedExtensions = {
        ".pdf", ".docx", ".doc", ".html", ".htm", ".xhtml", ".tex", ".txt"
    };

    public UploadValidator(IOptions<LimitsOptions> limitsOptions)
    {
        var limits = limitsOptions.Value;
        var maxBytes = limits.MaxFileMb * 1024 * 1024; // Convert MB to bytes

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        RuleFor(x => x.File)
            .Must(IsValidFileType)
            .WithMessage("File must be a PDF, DOCX, DOC, HTML, or TEX file")
            .When(x => x.File != null);

        RuleFor(x => x.File)
            .Must(file => file?.Length > 0)
            .WithMessage("File cannot be empty")
            .When(x => x.File != null);

        RuleFor(x => x.File)
            .Must(file => file?.Length <= maxBytes)
            .WithMessage($"File size must not exceed {limits.MaxFileMb} MB")
            .When(x => x.File != null);
    }

    private static bool IsValidFileType(IFormFile? file)
    {
        if (file == null) return false;

        // Check content type
        if (AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check file extension as fallback (browsers sometimes send incorrect MIME types)
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        return !string.IsNullOrEmpty(extension) && AllowedExtensions.Contains(extension);
    }
}

public class LimitsOptions
{
    public int MaxPdfMb { get; set; } = 50; // Keep for backward compatibility
    public int MaxFileMb { get; set; } = 50; // New property for all file types
}
