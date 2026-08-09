namespace StayHub.Web.Services;

/// <summary>
/// Saves uploads to the local <c>wwwroot/uploads</c> folder. Adequate for the
/// assignment; a production deployment would use blob storage instead.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private static readonly string[] AllowedExtensions =
        { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".pdf" };

    private const long MaxBytes = 5 * 1024 * 1024;

    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initialises a new instance of the <see cref="FileStorageService"/> class.
    /// </summary>
    /// <param name="environment">The hosting environment, used to locate <c>wwwroot</c>.</param>
    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <inheritdoc />
    public bool IsValid(IFormFile? file)
    {
        if (file is null || file.Length == 0 || file.Length > MaxBytes)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }

    /// <inheritdoc />
    public async Task<string> SaveAsync(IFormFile file, string subfolder)
    {
        if (!IsValid(file))
            throw new InvalidOperationException("The file is empty, too large, or of an unsupported type.");

        var root = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var folder = Path.Combine(root, "uploads", subfolder);
        Directory.CreateDirectory(folder);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(folder, storedName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{subfolder}/{storedName}";
    }
}
