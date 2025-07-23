using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Permissions;

/// <summary>
/// Implements <see cref="IPermissionPresentationMapper" /> for document property value permissions.
/// </summary>
public class DocumentPropertyValuePermissionMapper : IPermissionPresentationMapper, IPermissionMapper
{
    /// <inheritdoc/>
    public string Context => DocumentPropertyValueGranularPermission.ContextType;

    /// <inheritdoc/>
    public Type PresentationModelToHandle => typeof(DocumentPropertyValuePermissionPresentationModel);

    /// <inheritdoc/>
    public IGranularPermission MapFromDto(UserGroup2GranularPermissionDto dto) =>
        new DocumentPropertyValueGranularPermission()
        {
            Key = dto.UniqueId!.Value,
            Permission = dto.Permission,
        };

    /// <inheritdoc/>
    public IEnumerable<IPermissionPresentationModel> MapManyAsync(IEnumerable<IGranularPermission> granularPermissions)
    {
        var intermediate = granularPermissions.Where(p => p.Key.HasValue).Select(p =>
            {
                var parts = p.Permission.Split('|');
                return parts.Length == 2 && Guid.TryParse(parts[0], out Guid propertyTypeId)
                    ? new { DocumentTypeId = p.Key!.Value, PropertyTypeId = propertyTypeId, Verb = parts[1] }
                    : null;
            })
            .WhereNotNull()
            .ToArray();

        var intermediateByDocumentType = intermediate.GroupBy(x => x.DocumentTypeId);
        foreach (var documentTypeGroup in intermediateByDocumentType)
        {
            foreach (var propertyTypeGroup in documentTypeGroup.GroupBy(x => x.PropertyTypeId))
            {
                yield return new DocumentPropertyValuePermissionPresentationModel
                {
                    DocumentType = new ReferenceByIdModel(documentTypeGroup.Key),
                    PropertyType = new ReferenceByIdModel(propertyTypeGroup.Key),
                    Verbs = propertyTypeGroup
                        .Select(x => x.Verb)
                        .Where(verb => verb.IsNullOrWhiteSpace() is false)
                        .ToHashSet(),
                };
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IGranularPermission> MapToGranularPermissions(IPermissionPresentationModel permissionViewModel)
    {
        if (permissionViewModel is not DocumentPropertyValuePermissionPresentationModel documentTypePermissionPresentationModel)
        {
            yield break;
        }

        foreach (var verb in documentTypePermissionPresentationModel.Verbs.Distinct().DefaultIfEmpty(string.Empty))
        {
            yield return new DocumentPropertyValueGranularPermission
            {
                Key = documentTypePermissionPresentationModel.DocumentType.Id,
                Permission = $"{documentTypePermissionPresentationModel.PropertyType.Id}|{verb}"
            };
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IPermissionPresentationModel> AggregatePresentationModels(IUser user, IEnumerable<IPermissionPresentationModel> models)
    {
        IEnumerable<((Guid DocumentTypeId, Guid PropertyTypeId) Key, ISet<string> Verbs)> groupedModels = models
            .Cast<DocumentPropertyValuePermissionPresentationModel>()
            .GroupBy(x => (x.DocumentType.Id, x.PropertyType.Id))
            .Select(x => (x.Key, (ISet<string>)x.SelectMany(y => y.Verbs).Distinct().ToHashSet()));

        foreach (((Guid DocumentTypeId, Guid PropertyTypeId) key, ISet<string> verbs) in groupedModels)
        {
            yield return new DocumentPropertyValuePermissionPresentationModel
            {
                DocumentType = new ReferenceByIdModel(key.DocumentTypeId),
                PropertyType = new ReferenceByIdModel(key.PropertyTypeId),
                Verbs = verbs
            };
        }
    }
}
