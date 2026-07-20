namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a lightweight public access entry containing essential access control information.
/// </summary>
public class PublicAccessEntrySlim
{
    /// <summary>
    ///     Gets or sets the unique identifier of the protected content.
    /// </summary>
    public Guid ContentId { get; set; }

    /// <summary>
    ///     Gets or sets the usernames of members allowed to access the content.
    /// </summary>
    public string[] MemberUserNames { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     Gets or sets the names of member groups allowed to access the content.
    /// </summary>
    public string[] MemberGroupNames { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     Gets or sets the unique identifier of the login page.
    /// </summary>
    public Guid LoginPageId { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the error page shown for unauthorized access.
    /// </summary>
    public Guid ErrorPageId { get; set; }
}
