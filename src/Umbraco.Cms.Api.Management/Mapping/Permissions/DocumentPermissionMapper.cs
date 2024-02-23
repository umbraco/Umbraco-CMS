using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;

namespace Umbraco.Cms.Api.Management.Mapping.Permissions;

public class DocumentPermissionMapper : IPermissionPresentationMapper, IPermissionMapper
{
    public string Context => DocumentGranularPermission.ContextType;
    public IGranularPermission MapFromDto(UserGroup2GranularPermissionDto dto) =>
        new DocumentGranularPermission()
        {
            Key = dto.UniqueId!.Value,
            Permission = dto.Permission
        };

    public Type PresentationModelToHandle => typeof(DocumentPermissionPresentationModel);

    public IEnumerable<IPermissionPresentationModel> MapManyAsync(IEnumerable<IGranularPermission> granularPermissions)
    {
        IEnumerable<IGrouping<Guid?, IGranularPermission>> keyGroups = granularPermissions.GroupBy(x => x.Key);
        foreach (IGrouping<Guid?, IGranularPermission> keyGroup in keyGroups)
        {
            var verbs = keyGroup.Select(x => x.Permission).ToHashSet();
            if (keyGroup.Key.HasValue)
            {
                yield return new DocumentPermissionPresentationModel()
                {
                    Document = new ReferenceByIdModel(keyGroup.Key.Value),
                    Verbs = verbs
                };
            }
        }
    }

    public IEnumerable<IGranularPermission> MapToGranularPermissions(IPermissionPresentationModel permissionViewModel)
    {
        if (permissionViewModel is DocumentPermissionPresentationModel documentPermissionPresentationModel)
        {
            Guid key = documentPermissionPresentationModel.Document.Id;
            foreach (var verb in documentPermissionPresentationModel.Verbs)
            {
                yield return new DocumentGranularPermission()
                {
                    Key = documentPermissionPresentationModel.Document.Id,
                    Permission = verb
                };
            }
        }
    }
}
