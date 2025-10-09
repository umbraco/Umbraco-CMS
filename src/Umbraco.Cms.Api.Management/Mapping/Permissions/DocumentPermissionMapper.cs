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
    private readonly Lazy<IEntityService> _entityService;
    private readonly Lazy<IUserService> _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentPermissionMapper"/> class.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 17.")]
    public DocumentPermissionMapper()
        : this(
              StaticServiceProvider.Instance.GetRequiredService<Lazy<IEntityService>>(),
              StaticServiceProvider.Instance.GetRequiredService<Lazy<IUserService>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentPermissionMapper"/> class.
    /// </summary>
    public DocumentPermissionMapper(Lazy<IEntityService> entityService, Lazy<IUserService> userService)
    {
        _entityService = entityService;
        _userService = userService;
    }

    /// <inheritdoc/>
    public string Context => DocumentGranularPermission.ContextType;

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

        // Batch retrieve all documents by their keys.
        var documents = _entityService.Value.GetAll<IContent>(documentKeysWithGranularPermissions)
            .ToDictionary(doc => doc.Key, doc => doc.Path);

        // Iterate through each document key that has granular permissions.
        foreach (Guid documentKey in documentKeysWithGranularPermissions)
        {
            // Retrieve the path from the pre-fetched documents.
            if (!documents.TryGetValue(documentKey, out var path) || string.IsNullOrEmpty(path))
            {
                continue;
            }

            // With the path we can call the same logic as used server-side for authorizing access to resources.
            EntityPermissionSet permissionsForPath = _userService.Value.GetPermissionsForPath(user, path);
            yield return new DocumentPermissionPresentationModel
            {
                Document = new ReferenceByIdModel(documentKey),
                Verbs = permissionsForPath.GetAllPermissions(),
            };
        }
    }
}
