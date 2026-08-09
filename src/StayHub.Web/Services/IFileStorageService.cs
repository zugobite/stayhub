namespace StayHub.Web.Services;

/// <summary>
/// Stores user-supplied files (listing photographs and guest identity documents)
/// under <c>wwwroot</c> and returns the relative URL to persist.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves an uploaded file into the given <c>wwwroot</c> subfolder.
    /// </summary>
    /// <param name="file">The uploaded file.</param>
    /// <param name="subfolder">The subfolder beneath <c>wwwroot/uploads</c>.</param>
    /// <returns>The relative URL of the stored file.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the file is empty or its extension is not permitted.</exception>
    Task<string> SaveAsync(IFormFile file, string subfolder);

    /// <summary>
    /// Determines whether the file has a permitted extension and a workable size.
    /// </summary>
    /// <param name="file">The uploaded file.</param>
    /// <returns><c>true</c> when the file can be stored; otherwise <c>false</c>.</returns>
    bool IsValid(IFormFile? file);
}
