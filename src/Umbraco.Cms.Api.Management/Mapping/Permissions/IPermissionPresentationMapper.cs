using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core.Models.Membership.Permissions;

namespace Umbraco.Cms.Api.Management.Mapping.Permissions;

public interface IPermissionPresentationMapper
{
    string Context { get; }

    Type PresentationModelToHandle { get; }

    IEnumerable<IPermissionPresentationModel> MapManyAsync(IEnumerable<IGranularPermission> granularPermissions);

    IEnumerable<IGranularPermission> MapToGranularPermissions(IPermissionPresentationModel permissionViewModel);
}
