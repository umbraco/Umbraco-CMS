namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents the base model for member editing operations.
/// </summary>
public abstract class MemberEditingModelBase : ContentEditingModelBase
{
    /// <summary>
    ///     Gets or sets a value indicating whether the member is approved.
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    ///     Gets or sets the collection of role keys assigned to the member.
    /// </summary>
    public IEnumerable<Guid>? Roles { get; set; }

    /// <summary>
    ///     Gets or sets the email address of the member.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the username of the member.
    /// </summary>
    public string Username { get; set; } = string.Empty;
}
