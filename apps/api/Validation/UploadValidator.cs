using FluentValidation;
using Microsoft.Extensions.Options;

namespace Api.Validation;

public class UploadRequest
{
    public IFormFile File { get; set; } = null!;
}

public class UploadValidator : AbstractValidator<UploadRequest>
{
    public UploadValidator(IOptions<LimitsOptions> limitsOptions)
    {
        var limits = limitsOptions.Value;
        var maxBytes = limits.MaxPdfMb * 1024 * 1024; // Convert MB to bytes

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        RuleFor(x => x.File)
            .Must(file => file?.ContentType == "application/pdf")
            .WithMessage("File must be a PDF")
            .When(x => x.File != null);

        RuleFor(x => x.File)
            .Must(file => file?.Length > 0)
            .WithMessage("File cannot be empty")
            .When(x => x.File != null);

        RuleFor(x => x.File)
            .Must(file => file?.Length <= maxBytes)
            .WithMessage($"File size must not exceed {limits.MaxPdfMb} MB")
            .When(x => x.File != null);
    }
}

public class LimitsOptions
{
    public int MaxPdfMb { get; set; } = 50;
}
