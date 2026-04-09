namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents the base presentation model for a user.
/// </summary>
public class UserPresentationBase
{
    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username associated with the user.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the user.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of user group identifiers associated with the user.
    /// </summary>
    public ISet<ReferenceByIdModel> UserGroupIds { get; set; } = new HashSet<ReferenceByIdModel>();
}
