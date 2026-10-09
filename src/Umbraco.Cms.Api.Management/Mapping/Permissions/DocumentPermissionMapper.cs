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
/// Implements <see cref="IPermissionPresentationMapper" /> for document permissions.
/// </summary>
/// <remarks>
/// This mapping maps all the way from management api to database in one file intentionally, so it is very clear what it takes, if we wanna add permissions to media or other types in the future.
/// </remarks>
public class DocumentPermissionMapper : IPermissionPresentationMapper, IPermissionMapper
{
    private readonly Lazy<IContentPermissionService> _contentPermissionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentPermissionMapper"/> class.
    /// </summary>
    /// <param name="entityService">The entity service.</param>
    /// <param name="userService">The user service.</param>
    /// <param name="contentPermissionService">The content permission service.</param>
    // TODO (V19): Remove the entityService and userService parameters as they are not used in the current implementation.
    public DocumentPermissionMapper(
        Lazy<IEntityService> entityService,
        Lazy<IUserService> userService,
        Lazy<IContentPermissionService> contentPermissionService) => _contentPermissionService = contentPermissionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentPermissionMapper"/> class.
    /// </summary>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public DocumentPermissionMapper(Lazy<IEntityService> entityService, Lazy<IUserService> userService)
        : this(
            entityService,
            userService,
            new Lazy<IContentPermissionService>(StaticServiceProvider.Instance.GetRequiredService<IContentPermissionService>))
    {
    }

    /// <inheritdoc/>
    public string Context => DocumentGranularPermission.ContextType;

    /// <summary>
    /// Maps a <see cref="UserGroup2GranularPermissionDto"/> to an <see cref="IGranularPermission"/> instance.
    /// </summary>
    /// <param name="dto">The DTO containing user group granular permission data.</param>
    /// <returns>An <see cref="IGranularPermission"/> representing the mapped document permission.</returns>
    public IGranularPermission MapFromDto(UserGroup2GranularPermissionDto dto) =>
        new DocumentGranularPermission()
        {
            Key = dto.UniqueId!.Value,
            Permission = dto.Permission,
        };

    /// <inheritdoc/>
    public Type PresentationModelToHandle => typeof(DocumentPermissionPresentationModel);

    /// <inheritdoc/>
    public IEnumerable<IPermissionPresentationModel> MapManyAsync(IEnumerable<IGranularPermission> granularPermissions)
    {
        IEnumerable<IGrouping<Guid?, IGranularPermission>> keyGroups = granularPermissions.GroupBy(x => x.Key);
        foreach (IGrouping<Guid?, IGranularPermission> keyGroup in keyGroups)
        {
            var verbs = keyGroup.Select(x => x.Permission).ToHashSet();
            if (keyGroup.Key.HasValue)
            {
                yield return new DocumentPermissionPresentationModel
                {
                    Document = new ReferenceByIdModel(keyGroup.Key.Value),
                    Verbs = verbs,
                };
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IGranularPermission> MapToGranularPermissions(IPermissionPresentationModel permissionViewModel)
    {
        if (permissionViewModel is not DocumentPermissionPresentationModel documentPermissionPresentationModel)
        {
            yield break;
        }

        if (documentPermissionPresentationModel.Verbs.Any() is false || (documentPermissionPresentationModel.Verbs.Count == 1 && documentPermissionPresentationModel.Verbs.Contains(string.Empty)))
        {
            yield return new DocumentGranularPermission
            {
                Key = documentPermissionPresentationModel.Document.Id,
                Permission = string.Empty,
            };
            yield break;
        }

        foreach (var verb in documentPermissionPresentationModel.Verbs)
        {
            if (string.IsNullOrEmpty(verb))
            {
                continue;
            }

            yield return new DocumentGranularPermission
            {
                Key = documentPermissionPresentationModel.Document.Id,
                Permission = verb,
            };
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IPermissionPresentationModel> AggregatePresentationModels(IUser user, IEnumerable<IPermissionPresentationModel> models)
    {
        // Get the unique document keys that have granular permissions.
        Guid[] documentKeysWithGranularPermissions = models
            .Cast<DocumentPermissionPresentationModel>()
            .Select(x => x.Document.Id)
            .Distinct()
            .ToArray();

        // Resolve permissions through IContentPermissionService so custom implementations are respected.
        IEnumerable<NodePermissions> permissions = _contentPermissionService.Value
            .GetPermissionsAsync(user, documentKeysWithGranularPermissions)
            .GetAwaiter()
            .GetResult();

        foreach (NodePermissions nodePermission in permissions)
        {
            yield return new DocumentPermissionPresentationModel
            {
                Document = new ReferenceByIdModel(nodePermission.NodeKey),
                Verbs = nodePermission.Permissions,
            };
        }
    }
}
