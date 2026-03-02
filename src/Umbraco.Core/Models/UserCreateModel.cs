using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the model used for creating a new user.
/// </summary>
public class UserCreateModel
{
    /// <summary>
    ///     Gets or sets the optional unique identifier for the user.
    /// </summary>
    /// <value>
    ///     The user's GUID, or <c>null</c> to auto-generate one.
    /// </value>
    public Guid? Id { get; set; }

    /// <summary>
    ///     Gets or sets the email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the username for the user.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the display name of the user.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the kind/type of user being created.
    /// </summary>
    public UserKind Kind { get; set; }

    /// <summary>
    ///     Gets or sets the collection of user group keys the user should be assigned to.
    /// </summary>
    public ISet<Guid> UserGroupKeys { get; set; } = new HashSet<Guid>();
}
