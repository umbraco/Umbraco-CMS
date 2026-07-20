namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a model for creating a member.
/// </summary>
public class MemberCreateModel : MemberEditingModelBase
{
    /// <summary>
    ///     Gets or sets the password for the new member.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the optional unique key for the member being created.
    /// </summary>
    public Guid? Key { get; set; }

    /// <summary>
    ///     Gets or sets the key of the member type for the member being created.
    /// </summary>
    public Guid ContentTypeKey { get; set; } = Guid.Empty;
}
