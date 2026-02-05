namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the model used for updating an existing user.
/// </summary>
public class UserUpdateModel
{
    /// <summary>
    ///     Gets or sets the unique key of the existing user to update.
    /// </summary>
    public required Guid ExistingUserKey { get; set; }

    /// <summary>
    ///     Gets or sets the email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the username of the user.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the display name of the user.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the ISO code of the user's preferred language.
    /// </summary>
    public string LanguageIsoCode { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the collection of content start node keys for the user.
    /// </summary>
    /// <remarks>
    ///     These define the content tree sections the user has access to.
    /// </remarks>
    public ISet<Guid> ContentStartNodeKeys { get; set; } = new HashSet<Guid>();

    /// <summary>
    ///     Gets or sets a value indicating whether the user has access to the content root.
    /// </summary>
    public bool HasContentRootAccess { get; set; }

    /// <summary>
    ///     Gets or sets the collection of media start node keys for the user.
    /// </summary>
    /// <remarks>
    ///     These define the media tree sections the user has access to.
    /// </remarks>
    public ISet<Guid> MediaStartNodeKeys { get; set; } = new HashSet<Guid>();

    /// <summary>
    ///     Gets or sets a value indicating whether the user has access to the media root.
    /// </summary>
    public bool HasMediaRootAccess { get; set; }

    /// <summary>
    ///     Gets or sets the collection of user group keys the user belongs to.
    /// </summary>
    public ISet<Guid> UserGroupKeys { get; set; } = new HashSet<Guid>();
}
