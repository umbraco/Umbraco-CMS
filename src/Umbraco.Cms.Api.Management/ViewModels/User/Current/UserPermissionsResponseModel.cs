namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class UserPermissionsResponseModel
{
   public IEnumerable<UserPermissionViewModel> Permissions { get; set; } = Enumerable.Empty<UserPermissionViewModel>();
}
