using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Permissions;

public class DocumentTypePermissionMapper : IPermissionPresentationMapper, IPermissionMapper
{
    public string Context => DocumentTypeGranularPermission.ContextType;

    public IGranularPermission MapFromDto(UserGroup2GranularPermissionDto dto) =>
        new DocumentTypeGranularPermission()
        {
            Key = dto.UniqueId!.Value,
            Permission = dto.Permission,
        };

    public Type PresentationModelToHandle => typeof(DocumentTypePermissionPresentationModel);

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
                yield return new DocumentTypePermissionPresentationModel
                {
                    DocumentType = new ReferenceByIdModel(documentTypeGroup.Key),
                    PropertyType = new ReferenceByIdModel(propertyTypeGroup.Key),
                    Verbs = propertyTypeGroup.Select(x => x.Verb).ToHashSet(),
                };
            }
        }
    }

    public IEnumerable<IGranularPermission> MapToGranularPermissions(IPermissionPresentationModel permissionViewModel)
    {
        if (permissionViewModel is not DocumentTypePermissionPresentationModel documentTypePermissionPresentationModel)
        {
            yield break;
        }

        foreach (var verb in documentTypePermissionPresentationModel.Verbs)
        {
            if (string.IsNullOrEmpty(verb))
            {
                continue;
            }

            yield return new DocumentTypeGranularPermission
            {
                Key = documentTypePermissionPresentationModel.DocumentType.Id,
                Permission = $"{documentTypePermissionPresentationModel.PropertyType.Id}|{verb}"
            };
        }
    }
}
