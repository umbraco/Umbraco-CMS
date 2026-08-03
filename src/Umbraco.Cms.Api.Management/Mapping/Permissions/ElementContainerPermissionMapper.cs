using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;

namespace Umbraco.Cms.Api.Management.Mapping.Permissions;

/// <summary>
/// Implements <see cref="IPermissionPresentationMapper" /> for element container permissions.
/// </summary>
/// <remarks>
/// Verbs stored under this context can be a mix of element-container-structural verbs (e.g.
/// <c>Umb.ElementContainer.Create</c>) and descendant-element verbs (e.g. <c>Umb.Element.Create</c>) —
/// both apply to the same node path, and runtime permission resolution does not distinguish them by context.
/// </remarks>
public class ElementContainerPermissionMapper : IPermissionPresentationMapper, IPermissionMapper
{
    private readonly Lazy<IElementContainerPermissionService> _elementContainerPermissionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementContainerPermissionMapper"/> class.
    /// </summary>
    /// <param name="elementContainerPermissionService">The element container permission service.</param>
    public ElementContainerPermissionMapper(Lazy<IElementContainerPermissionService> elementContainerPermissionService)
        => _elementContainerPermissionService = elementContainerPermissionService;

    /// <inheritdoc cref="IPermissionMapper" />
    public string Context => ElementContainerGranularPermission.ContextType;

    /// <inheritdoc/>
    public Type PresentationModelToHandle => typeof(ElementContainerPermissionPresentationModel);

    /// <inheritdoc />
    public IGranularPermission MapFromDto(UserGroup2GranularPermissionDto dto) =>
        new ElementContainerGranularPermission
        {
            Key = dto.UniqueId!.Value,
            Permission = dto.Permission,
        };

    /// <inheritdoc/>
    public IEnumerable<IPermissionPresentationModel> MapManyAsync(IEnumerable<IGranularPermission> granularPermissions)
    {
        IEnumerable<IGrouping<Guid?, IGranularPermission>> keyGroups = granularPermissions.GroupBy(x => x.Key);
        foreach (IGrouping<Guid?, IGranularPermission> keyGroup in keyGroups)
        {
            var verbs = keyGroup.Select(x => x.Permission).ToHashSet();
            if (keyGroup.Key.HasValue)
            {
                yield return new ElementContainerPermissionPresentationModel
                {
                    ElementContainer = new ReferenceByIdModel(keyGroup.Key.Value),
                    Verbs = verbs,
                };
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IGranularPermission> MapToGranularPermissions(IPermissionPresentationModel permissionViewModel)
    {
        if (permissionViewModel is not ElementContainerPermissionPresentationModel elementContainerPermissionPresentationModel)
        {
            yield break;
        }

        if (elementContainerPermissionPresentationModel.Verbs.Any() is false
            || (elementContainerPermissionPresentationModel.Verbs.Count == 1
                && elementContainerPermissionPresentationModel.Verbs.Contains(string.Empty)))
        {
            yield return new ElementContainerGranularPermission
            {
                Key = elementContainerPermissionPresentationModel.ElementContainer.Id,
                Permission = string.Empty,
            };
            yield break;
        }

        foreach (var verb in elementContainerPermissionPresentationModel.Verbs)
        {
            if (string.IsNullOrEmpty(verb))
            {
                continue;
            }

            yield return new ElementContainerGranularPermission
            {
                Key = elementContainerPermissionPresentationModel.ElementContainer.Id,
                Permission = verb,
            };
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IPermissionPresentationModel> AggregatePresentationModels(IUser user, IEnumerable<IPermissionPresentationModel> models)
    {
        Guid[] containerKeysWithGranularPermissions = models
            .Cast<ElementContainerPermissionPresentationModel>()
            .Select(x => x.ElementContainer.Id)
            .Distinct()
            .ToArray();

        IEnumerable<NodePermissions> permissions = _elementContainerPermissionService.Value
            .GetPermissionsAsync(user, containerKeysWithGranularPermissions)
            .GetAwaiter()
            .GetResult();

        foreach (NodePermissions nodePermission in permissions)
        {
            yield return new ElementContainerPermissionPresentationModel
            {
                ElementContainer = new ReferenceByIdModel(nodePermission.NodeKey),
                Verbs = nodePermission.Permissions,
            };
        }
    }
}
