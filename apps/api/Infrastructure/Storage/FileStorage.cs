using Microsoft.Extensions.Options;

namespace Api.Infrastructure.Storage;

public interface IFileStorage
{
    Task<string> SaveAsync(Guid paperId, Stream stream, string? originalFileName = null);
    string GetPath(Guid paperId, string? originalFileName = null);
    bool Exists(Guid paperId);
    void EnsureDirectoryExists();
}

public class FileStorage : IFileStorage
{
    private readonly StorageOptions _options;

    public FileStorage(IOptions<StorageOptions> options)
    {
        _options = options.Value;
        EnsureDirectoryExists();
    }

    public async Task<string> SaveAsync(Guid paperId, Stream stream, string? originalFileName = null)
    {
        var filePath = GetPath(paperId, originalFileName);
        var directory = Path.GetDirectoryName(filePath);
        
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream);
        
        return filePath;
    }

    public string GetPath(Guid paperId, string? originalFileName = null)
    {
        if (string.IsNullOrEmpty(originalFileName))
        {
            return Path.Combine(_options.FilesRoot, $"{paperId}.pdf");
        }
        
        var extension = Path.GetExtension(originalFileName);
        return Path.Combine(_options.FilesRoot, $"{paperId}{extension}");
    }

    public bool Exists(Guid paperId)
    {
        return File.Exists(GetPath(paperId));
    }

    public void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_options.FilesRoot))
        {
            Directory.CreateDirectory(_options.FilesRoot);
        }
    }
}

public class StorageOptions
{
    public string FilesRoot { get; set; } = string.Empty;
}
