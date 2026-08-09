namespace StayHub.Web.ViewModels;

/// <summary>
/// Backs a short legal policy page linked from the site footer.
/// </summary>
public class LegalPageViewModel
{
    /// <summary>Gets or sets the page title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets short intro copy shown under the title.</summary>
    public string Intro { get; set; } = string.Empty;

    /// <summary>Gets or sets policy sections (heading + body paragraphs).</summary>
    public IReadOnlyList<LegalSection> Sections { get; set; } = Array.Empty<LegalSection>();
}

/// <summary>A single heading and body block on a legal page.</summary>
public record LegalSection(string Heading, string Body);
