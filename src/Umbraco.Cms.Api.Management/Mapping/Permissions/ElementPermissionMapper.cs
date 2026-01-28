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
/// Implements <see cref="IPermissionPresentationMapper" /> for element permissions.
/// </summary>
/// <remarks>
/// This mapping maps all the way from management api to database in one file intentionally, so it is very clear what it takes, if we wanna add permissions to media or other types in the future.
/// </remarks>
public class ElementPermissionMapper : IPermissionPresentationMapper, IPermissionMapper
{
    private readonly Lazy<IEntityService> _entityService;
    private readonly Lazy<IUserService> _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementPermissionMapper"/> class.
    /// </summary>
    public ElementPermissionMapper(Lazy<IEntityService> entityService, Lazy<IUserService> userService)
    {
        _entityService = entityService;
        _userService = userService;
    }

    /// <inheritdoc cref="IPermissionMapper" />
    public string Context => ElementGranularPermission.ContextType;

    /// <inheritdoc/>
    public Type PresentationModelToHandle => typeof(ElementPermissionPresentationModel);

    /// <inheritdoc />
    public IGranularPermission MapFromDto(UserGroup2GranularPermissionDto dto) =>
        new ElementGranularPermission
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
                yield return new ElementPermissionPresentationModel
                {
                    Element = new ReferenceByIdModel(keyGroup.Key.Value),
                    Verbs = verbs,
                };
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IGranularPermission> MapToGranularPermissions(IPermissionPresentationModel permissionViewModel)
    {
        if (permissionViewModel is not ElementPermissionPresentationModel elementPermissionPresentationModel)
        {
            yield break;
        }

        if (elementPermissionPresentationModel.Verbs.Any() is false
            || (elementPermissionPresentationModel.Verbs.Count == 1
                && elementPermissionPresentationModel.Verbs.Contains(string.Empty)))
        {
            yield return new ElementGranularPermission
            {
                Key = elementPermissionPresentationModel.Element.Id,
                Permission = string.Empty,
            };
            yield break;
        }

        foreach (var verb in elementPermissionPresentationModel.Verbs)
        {
            if (string.IsNullOrEmpty(verb))
            {
                continue;
            }

            yield return new ElementGranularPermission
            {
                Key = elementPermissionPresentationModel.Element.Id,
                Permission = verb,
            };
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IPermissionPresentationModel> AggregatePresentationModels(IUser user, IEnumerable<IPermissionPresentationModel> models)
    {
        // Get the unique element keys that have granular permissions.
        Guid[] elementKeysWithGranularPermissions = models
            .Cast<ElementPermissionPresentationModel>()
            .Select(x => x.Element.Id)
            .Distinct()
            .ToArray();

        // Batch retrieve all elements by their keys.
        var elements = _entityService.Value.GetAll<IElement>(elementKeysWithGranularPermissions)
            .ToDictionary(elem => elem.Key, elem => elem.Path);

        // Iterate through each element key that has granular permissions.
        foreach (Guid elementKey in elementKeysWithGranularPermissions)
        {
            // Retrieve the path from the pre-fetched elements.
            if (!elements.TryGetValue(elementKey, out var path) || string.IsNullOrEmpty(path))
            {
                continue;
            }

            // With the path we can call the same logic as used server-side for authorizing access to resources.
            EntityPermissionSet permissionsForPath = _userService.Value.GetPermissionsForPath(user, path);
            yield return new ElementPermissionPresentationModel
            {
                Element = new ReferenceByIdModel(elementKey),
                Verbs = permissionsForPath.GetAllPermissions(),
            };
        }
    }
}
