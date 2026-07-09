namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

/// <summary>
/// Represents a view model containing the permissions assigned to the current user within the management API context.
/// </summary>
public class UserPermissionViewModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the node associated with the user permission.
    /// </summary>
    public Guid NodeKey { get; set; }

    /// <summary>
    /// Gets or sets the set of permission identifiers assigned to the user.
    /// Each permission is represented as a string code.
    /// </summary>
    public ISet<string> Permissions { get; set; } = new HashSet<string>();
}
