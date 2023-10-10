using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class UserPermissionsResponseModel : IResponseModel
{
   public IEnumerable<UserPermissionViewModel> Permissions { get; set; } = Array.Empty<UserPermissionViewModel>();
   public string Type => UdiEntityType.UserPermissions;
}
