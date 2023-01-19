using Umbraco.Cms.Api.Management.Mapping;
using Umbraco.Cms.Api.Management.ViewModels.UserGroups;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Api.Management.Factories;

/// <inheritdoc />
public class UserGroupViewModelFactory : IUserGroupViewModelFactory
{
    private readonly IEntityService _entityService;
    private readonly IShortStringHelper _shortStringHelper;

    public UserGroupViewModelFactory(IEntityService entityService, IShortStringHelper shortStringHelper)
    {
        _entityService = entityService;
        _shortStringHelper = shortStringHelper;
    }

    /// <inheritdoc />
    public UserGroupViewModel Create(IUserGroup userGroup)
    {
        // We have to get the entity of the start content node and start media node
        // In order to be able to get the key.
        Guid? contentStartNodeKey = GetKeyFromId(userGroup.StartContentId, UmbracoObjectTypes.Document);
        Guid? mediaStartNodeKey = GetKeyFromId(userGroup.StartMediaId, UmbracoObjectTypes.Media);

        return new UserGroupViewModel
        {
            Name = userGroup.Name ?? string.Empty,
            Key = userGroup.Key,
            DocumentStartNodeKey = contentStartNodeKey,
            MediaStartNodeKey = mediaStartNodeKey,
            Icon = userGroup.Icon,
            Languages = userGroup.AllowedLanguages,
            HasAccessToAllLanguages = userGroup.HasAccessToAllLanguages,
            Permissions = userGroup.PermissionNames,
            Sections = userGroup.AllowedSections.Select(SectionMapper.GetName),
        };

    }

    public IEnumerable<UserGroupViewModel> CreateMultiple(IEnumerable<IUserGroup> userGroups) =>
        userGroups.Select(Create);

    // TODO: Should we split this class out? Maybe rename it?
    public IUserGroup Create(UserGroupSaveModel saveModel)
    {
        int? contentStartNodeId = GetIdFromKey(saveModel.DocumentStartNodeKey, UmbracoObjectTypes.Document);
        int? mediaStartNodeId = GetIdFromKey(saveModel.MediaStartNodeKey, UmbracoObjectTypes.Media);

        var group = new UserGroup(_shortStringHelper)
        {
            Name = saveModel.Name,
            Alias = saveModel.Name,
            Icon = saveModel.Icon,
            HasAccessToAllLanguages = saveModel.HasAccessToAllLanguages,
            PermissionNames = saveModel.Permissions,
            StartContentId = contentStartNodeId,
            StartMediaId = mediaStartNodeId,
        };

        foreach (var section in saveModel.Sections)
        {
            group.AddAllowedSection(SectionMapper.GetAlias(section));
        }

        foreach (var language in saveModel.Languages)
        {
            group.AddAllowedLanguage(language);
        }

        return group;
    }

    public IUserGroup Update(IUserGroup current, UserGroupUpdateModel update)
    {
        current.Name = update.Name;
        current.Icon = update.Icon;
        current.HasAccessToAllLanguages = update.HasAccessToAllLanguages;
        current.StartContentId = GetIdFromKey(update.DocumentStartNodeKey, UmbracoObjectTypes.Document);
        current.StartMediaId = GetIdFromKey(update.MediaStartNodeKey, UmbracoObjectTypes.Media);
        current.PermissionNames = update.Permissions;

        current.ClearAllowedLanguages();
        foreach (var languageId in update.Languages)
        {
            current.AddAllowedLanguage(languageId);
        }

        current.ClearAllowedSections();
        foreach (var sectionName in update.Sections)
        {
            current.AddAllowedSection(SectionMapper.GetAlias(sectionName));
        }

        return current;
    }

    private Guid? GetKeyFromId(int? id, UmbracoObjectTypes objectType)
    {
        if (id is null)
        {
            return null;
        }

        Attempt<Guid> attempt = _entityService.GetKey(id.Value, objectType);
        if (attempt.Success is false)
        {
            return null;
        }

        return attempt.Result;
    }

    private int? GetIdFromKey(Guid? key, UmbracoObjectTypes objectType)
    {
        if (key is null)
        {
            return null;
        }

        Attempt<int> attempt = _entityService.GetId(key.Value, objectType);

        if (attempt.Success is false)
        {
            return null;
        }

        return attempt.Result;
    }
}
