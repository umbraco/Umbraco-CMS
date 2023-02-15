using Umbraco.Cms.Api.Management.Mapping;
using Umbraco.Cms.Api.Management.ViewModels.UserGroups;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

/// <inheritdoc />
public class UserGroupViewModelFactory : IUserGroupViewModelFactory
{
    private readonly IEntityService _entityService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ILanguageService _languageService;

    public UserGroupViewModelFactory(
        IEntityService entityService,
        IShortStringHelper shortStringHelper,
        ILanguageService languageService)
    {
        _entityService = entityService;
        _shortStringHelper = shortStringHelper;
        _languageService = languageService;
    }

    /// <inheritdoc />
    public async Task<UserGroupViewModel> CreateAsync(IUserGroup userGroup)
    {
        Guid? contentStartNodeKey = GetKeyFromId(userGroup.StartContentId, UmbracoObjectTypes.Document);
        Guid? mediaStartNodeKey = GetKeyFromId(userGroup.StartMediaId, UmbracoObjectTypes.Media);
        IEnumerable<string> languageIsoCodes = await MapLanguageIdsToIsoCodeAsync(userGroup.AllowedLanguages);

        return new UserGroupViewModel
        {
            Name = userGroup.Name ?? string.Empty,
            Key = userGroup.Key,
            DocumentStartNodeKey = contentStartNodeKey,
            MediaStartNodeKey = mediaStartNodeKey,
            Icon = userGroup.Icon,
            Languages = languageIsoCodes,
            HasAccessToAllLanguages = userGroup.HasAccessToAllLanguages,
            Permissions = userGroup.PermissionNames,
            Sections = userGroup.AllowedSections.Select(SectionMapper.GetName),
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserGroupViewModel>> CreateMultipleAsync(IEnumerable<IUserGroup> userGroups)
    {
        var userGroupViewModels = new List<UserGroupViewModel>();
        foreach (IUserGroup userGroup in userGroups)
        {
            userGroupViewModels.Add(await CreateAsync(userGroup));
        }

        return userGroupViewModels;
    }

    /// <inheritdoc />
    public async Task<Attempt<IUserGroup, UserGroupOperationStatus>> CreateAsync(UserGroupSaveModel saveModel)
    {
        var cleanedName = saveModel.Name.CleanForXss('[', ']', '(', ')', ':');

        var group = new UserGroup(_shortStringHelper)
        {
            Name = cleanedName,
            Alias = cleanedName,
            Icon = saveModel.Icon,
            HasAccessToAllLanguages = saveModel.HasAccessToAllLanguages,
            PermissionNames = saveModel.Permissions,
        };

        Attempt<UserGroupOperationStatus> assignmentAttempt = AssignStartNodesToUserGroup(saveModel, group);
        if (assignmentAttempt.Success is false)
        {
            return Attempt.FailWithStatus<IUserGroup, UserGroupOperationStatus>(assignmentAttempt.Result, group);
        }

        foreach (var section in saveModel.Sections)
        {
            group.AddAllowedSection(SectionMapper.GetAlias(section));
        }

        foreach (var languageId in await MapLanguageIsoCodesToIdsAsync(saveModel.Languages))
        {
            group.AddAllowedLanguage(languageId);
        }

        return Attempt.SucceedWithStatus<IUserGroup, UserGroupOperationStatus>(UserGroupOperationStatus.Success, group);
    }

    /// <inheritdoc />
    public async Task<Attempt<IUserGroup, UserGroupOperationStatus>> UpdateAsync(IUserGroup current, UserGroupUpdateModel update)
    {
        Attempt<UserGroupOperationStatus> assignmentAttempt = AssignStartNodesToUserGroup(update, current);
        if (assignmentAttempt.Success is false)
        {
            return Attempt.FailWithStatus(assignmentAttempt.Result, current);
        }

        current.Name = update.Name.CleanForXss('[', ']', '(', ')', ':');
        current.Icon = update.Icon;
        current.HasAccessToAllLanguages = update.HasAccessToAllLanguages;
        current.PermissionNames = update.Permissions;

        current.ClearAllowedLanguages();
        foreach (var languageId in await MapLanguageIsoCodesToIdsAsync(update.Languages))
        {
            current.AddAllowedLanguage(languageId);
        }

        current.ClearAllowedSections();
        foreach (var sectionName in update.Sections)
        {
            current.AddAllowedSection(SectionMapper.GetAlias(sectionName));
        }

        return Attempt.SucceedWithStatus(UserGroupOperationStatus.Success, current);
    }

    private async Task<IEnumerable<string>> MapLanguageIdsToIsoCodeAsync(IEnumerable<int> ids)
    {
        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();
        return languages
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.IsoCode);
    }

    private async Task<IEnumerable<int>> MapLanguageIsoCodesToIdsAsync(IEnumerable<string> isoCodes)
    {
        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();
        return languages
            .Where(x => isoCodes.Contains(x.IsoCode))
            .Select(x => x.Id);
    }

    private Attempt<UserGroupOperationStatus> AssignStartNodesToUserGroup(UserGroupBase source, IUserGroup target)
    {
        if (source.DocumentStartNodeKey is not null)
        {
            var contentId = GetIdFromKey(source.DocumentStartNodeKey.Value, UmbracoObjectTypes.Document);

            if (contentId is null)
            {
                return Attempt.Fail(UserGroupOperationStatus.DocumentStartNodeKeyNotFound);
            }

            target.StartContentId = contentId;
        }

        if (source.MediaStartNodeKey is not null)
        {
            var mediaId = GetIdFromKey(source.MediaStartNodeKey.Value, UmbracoObjectTypes.Media);

            if (mediaId is null)
            {
                return Attempt.Fail(UserGroupOperationStatus.MediaStartNodeKeyNotFound);
            }

            target.StartMediaId = mediaId;
        }

        return Attempt.Succeed(UserGroupOperationStatus.Success);
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

    private int? GetIdFromKey(Guid key, UmbracoObjectTypes objectType)
    {
        Attempt<int> attempt = _entityService.GetId(key, objectType);

        if (attempt.Success is false)
        {
            return null;
        }

        return attempt.Result;
    }
}
