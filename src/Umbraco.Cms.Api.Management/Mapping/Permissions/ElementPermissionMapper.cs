using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core.DependencyInjection;
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
    private readonly Lazy<IElementPermissionService> _elementPermissionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementPermissionMapper"/> class.
    /// </summary>
    /// <param name="entityService">The entity service.</param>
    /// <param name="userService">The user service.</param>
    /// <param name="elementPermissionService">The element permission service.</param>
    // TODO (V20): Remove the entityService and userService parameters as they are not used in the current implementation.
    public ElementPermissionMapper(
#pragma warning disable IDE0060 // Remove unused parameter
        Lazy<IEntityService> entityService,
        Lazy<IUserService> userService,
#pragma warning restore IDE0060 // Remove unused parameter
        Lazy<IElementPermissionService> elementPermissionService) => _elementPermissionService = elementPermissionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementPermissionMapper"/> class.
    /// </summary>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 20.")]
    public ElementPermissionMapper(Lazy<IEntityService> entityService, Lazy<IUserService> userService)
        : this(
            entityService,
            userService,
            new Lazy<IElementPermissionService>(StaticServiceProvider.Instance.GetRequiredService<IElementPermissionService>))
    {
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

        // Resolve permissions through IElementPermissionService so custom implementations are respected.
        IEnumerable<NodePermissions> permissions = _elementPermissionService.Value
            .GetPermissionsAsync(user, elementKeysWithGranularPermissions)
            .GetAwaiter()
            .GetResult();

        foreach (NodePermissions nodePermission in permissions)
        {
            yield return new ElementPermissionPresentationModel
            {
                Element = new ReferenceByIdModel(nodePermission.NodeKey),
                Verbs = nodePermission.Permissions,
            };
        }
    }
}
