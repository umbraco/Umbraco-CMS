using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;

namespace Umbraco.Cms.Api.Management.Mapping.Permissions;

/// <summary>
/// Defines methods for mapping and aggregating granular permissions to presentation models.
/// </summary>
public interface IPermissionPresentationMapper
{
    /// <summary>
    /// Gets the context type for the permissions being handled by this mapper.
    /// </summary>
    string Context { get; }

    /// <summary>
    /// Gets the type of the presentation model that this mapper handles.
    /// </summary>
    Type PresentationModelToHandle { get; }

    /// <summary>
    /// Maps a granular permission entity to a granular permission model.
    /// </summary>
    IEnumerable<IPermissionPresentationModel> MapManyAsync(IEnumerable<IGranularPermission> granularPermissions);

    /// <summary>
    /// Maps a granular permission to a granular permission model.
    /// </summary>
    IEnumerable<IGranularPermission> MapToGranularPermissions(IPermissionPresentationModel permissionViewModel);

    /// <summary>
    /// Aggregates multiple permission presentation models into a collection containing only one item per entity with aggregated permissions.
    /// </summary>
    IEnumerable<IPermissionPresentationModel> AggregatePresentationModels(IUser user, IEnumerable<IPermissionPresentationModel> models) => [];
}
