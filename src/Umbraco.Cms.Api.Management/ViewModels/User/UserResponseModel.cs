using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents a user response model in the Umbraco CMS Management API.
/// </summary>
public class UserResponseModel : UserPresentationBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the ISO code of the user's language.</summary>
    public string? LanguageIsoCode { get; set; }

    /// <summary>
    /// Gets or sets the collection of content start node references (by ID) that define the root documents accessible to the user.
    /// </summary>
    public ISet<ReferenceByIdModel> DocumentStartNodeIds { get; set; } = new HashSet<ReferenceByIdModel>();

    /// <summary>
    /// Gets or sets a value indicating whether the user can access the root of the document tree.
    /// </summary>
    public bool HasDocumentRootAccess { get; set; }

    /// <summary>
    /// Gets or sets the collection of media start node identifiers assigned to the user.
    /// Each identifier references a media node that serves as a starting point for the user's media access.
    /// </summary>
    public ISet<ReferenceByIdModel> MediaStartNodeIds { get; set; } = new HashSet<ReferenceByIdModel>();

    /// <summary>
    /// Gets or sets a value indicating whether the user has access to the media root.
    /// </summary>
    public bool HasMediaRootAccess { get; set; }

    /// <summary>
    /// Gets or sets a collection of URLs pointing to the user's avatar images.
    /// </summary>
    public IEnumerable<string> AvatarUrls { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets the current state of the user, such as active, disabled, or locked.
    /// </summary>
    public UserState State { get; set; }

    /// <summary>Gets or sets the number of failed login attempts for the user.</summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was created.
    /// </summary>
    public DateTimeOffset CreateDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was last updated.
    /// </summary>
    public DateTimeOffset UpdateDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the user's last login.
    /// </summary>
    public DateTimeOffset? LastLoginDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was last locked out.
    /// </summary>
    public DateTimeOffset? LastLockoutDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user last changed their password.
    /// </summary>
    public DateTimeOffset? LastPasswordChangeDate { get; set; }

    /// <summary>Gets or sets a value indicating whether the user is an administrator.</summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// Gets or sets the type of user, indicating their role or classification within the system.
    /// </summary>
    public UserKind Kind { get; set; }
}
