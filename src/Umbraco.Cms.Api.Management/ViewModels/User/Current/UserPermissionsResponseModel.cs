namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

/// <summary>
/// Represents a response model for the current user's permissions.
/// </summary>
public class UserPermissionsResponseModel
{
    /// <summary>
    /// Gets or sets the collection of user permissions.
    /// </summary>
   public IEnumerable<UserPermissionViewModel> Permissions { get; set; } = Enumerable.Empty<UserPermissionViewModel>();
}
