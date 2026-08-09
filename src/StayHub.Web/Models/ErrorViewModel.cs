namespace StayHub.Web.Models;

/// <summary>
/// Backs the shared error page.
/// </summary>
public class ErrorViewModel
{
    /// <summary>Gets or sets the request identifier used for correlating logs.</summary>
    public string? RequestId { get; set; }

    /// <summary>Gets a value indicating whether a request identifier is available.</summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
