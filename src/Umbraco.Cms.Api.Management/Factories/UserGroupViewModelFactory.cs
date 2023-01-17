using Umbraco.Cms.Api.Management.ViewModels.UserGroups;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

/// <inheritdoc />
public class UserGroupViewModelFactory : IUserGroupViewModelFactory
{
    private readonly IEntityService _entityService;

    public UserGroupViewModelFactory(IEntityService entityService) => _entityService = entityService;

    /// <inheritdoc />
    public UserGroupViewModel Create(IUserGroup userGroup)
    {
        // We have to get the entity of the start content node and start media node
        // In order to be able to get the key.
        Guid? contentStartNodeKey = GetKeyFromId(userGroup.StartContentId);
        Guid? mediaStartNodeKey = GetKeyFromId(userGroup.StartMediaId);

        return new UserGroupViewModel
        {
            Name = userGroup.Name,
            Key = userGroup.Key,
            ContentStartNodeKey = contentStartNodeKey,
            MediaStartNodeKey = mediaStartNodeKey,
            Icon = userGroup.Icon,
            Languages = userGroup.AllowedLanguages,
            HasAccessToAllLanguages = userGroup.HasAccessToAllLanguages,
            Permissions = Array.Empty<string>(), // TODO: populate this
            Sections = userGroup.AllowedSections, // TODO: Map to appropriate section names.
        };

    }

    private Guid? GetKeyFromId(int? id)
    {
        if (id is null)
        {
            return null;
        }

        IEntitySlim? entity = _entityService.Get(id.Value);

        return entity?.Key;
    }
}
