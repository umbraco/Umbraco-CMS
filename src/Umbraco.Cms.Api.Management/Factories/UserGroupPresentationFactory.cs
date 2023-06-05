using Umbraco.Cms.Api.Management.Mapping;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

/// <inheritdoc />
public class UserGroupPresentationFactory : IUserGroupPresentationFactory
{
    private readonly IEntityService _entityService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ILanguageService _languageService;

    public UserGroupPresentationFactory(
        IEntityService entityService,
        IShortStringHelper shortStringHelper,
        ILanguageService languageService)
    {
        _entityService = entityService;
        _shortStringHelper = shortStringHelper;
        _languageService = languageService;
    }

    /// <inheritdoc />
    public async Task<UserGroupResponseModel> CreateAsync(IUserGroup userGroup)
    {
        Guid? contentStartNodeKey = GetKeyFromId(userGroup.StartContentId, UmbracoObjectTypes.Document);
        Guid? mediaStartNodeKey = GetKeyFromId(userGroup.StartMediaId, UmbracoObjectTypes.Media);
        Attempt<IEnumerable<string>, UserGroupOperationStatus> languageIsoCodesMappingAttempt = await MapLanguageIdsToIsoCodeAsync(userGroup.AllowedLanguages);

        // We've gotten this data from the database, so the mapping should not fail
        if (languageIsoCodesMappingAttempt.Success is false)
        {
            throw new InvalidOperationException($"Unknown language ID in User Group: {userGroup.Name}");
        }

        return new UserGroupResponseModel
        {
            Name = userGroup.Name ?? string.Empty,
            Id = userGroup.Key,
            DocumentStartNodeId = contentStartNodeKey,
            MediaStartNodeId = mediaStartNodeKey,
            Icon = userGroup.Icon,
            Languages = languageIsoCodesMappingAttempt.Result,
            HasAccessToAllLanguages = userGroup.HasAccessToAllLanguages,
            Permissions = userGroup.PermissionNames,
            Sections = userGroup.AllowedSections.Select(SectionMapper.GetName),
        };
    }
    /// <inheritdoc />
    public async Task<UserGroupResponseModel> CreateAsync(IReadOnlyUserGroup userGroup)
    {
        // TODO figure out how to reuse code from Task<UserGroupResponseModel> CreateAsync(IUserGroup userGroup) instead of copying
        Guid? contentStartNodeKey = GetKeyFromId(userGroup.StartContentId, UmbracoObjectTypes.Document);
        Guid? mediaStartNodeKey = GetKeyFromId(userGroup.StartMediaId, UmbracoObjectTypes.Media);
        Attempt<IEnumerable<string>, UserGroupOperationStatus> languageIsoCodesMappingAttempt = await MapLanguageIdsToIsoCodeAsync(userGroup.AllowedLanguages);

        // We've gotten this data from the database, so the mapping should not fail
        if (languageIsoCodesMappingAttempt.Success is false)
        {
            throw new InvalidOperationException($"Unknown language ID in User Group: {userGroup.Name}");
        }

        return new UserGroupResponseModel
        {
            Name = userGroup.Name ?? string.Empty,
            Id = userGroup.Key,
            DocumentStartNodeId = contentStartNodeKey,
            MediaStartNodeId = mediaStartNodeKey,
            Icon = userGroup.Icon,
            Languages = languageIsoCodesMappingAttempt.Result,
            HasAccessToAllLanguages = userGroup.HasAccessToAllLanguages,
            Permissions = userGroup.PermissionNames,
            Sections = userGroup.AllowedSections.Select(SectionMapper.GetName),
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserGroupResponseModel>> CreateMultipleAsync(IEnumerable<IUserGroup> userGroups)
    {
        var userGroupViewModels = new List<UserGroupResponseModel>();
        foreach (IUserGroup userGroup in userGroups)
        {
            userGroupViewModels.Add(await CreateAsync(userGroup));
        }

        return userGroupViewModels;
    }
    /// <inheritdoc />
    public async Task<IEnumerable<UserGroupResponseModel>> CreateMultipleAsync(IEnumerable<IReadOnlyUserGroup> userGroups)
    {
        var userGroupViewModels = new List<UserGroupResponseModel>();
        foreach (IReadOnlyUserGroup userGroup in userGroups)
        {
            userGroupViewModels.Add(await CreateAsync(userGroup));
        }

        return userGroupViewModels;
    }

    /// <inheritdoc />
    public async Task<Attempt<IUserGroup, UserGroupOperationStatus>> CreateAsync(CreateUserGroupRequestModel requestModel)
    {
        var cleanedName = requestModel.Name.CleanForXss('[', ']', '(', ')', ':');

        var group = new UserGroup(_shortStringHelper)
        {
            Name = cleanedName,
            Alias = cleanedName,
            Icon = requestModel.Icon,
            HasAccessToAllLanguages = requestModel.HasAccessToAllLanguages,
            PermissionNames = requestModel.Permissions,
        };

        Attempt<UserGroupOperationStatus> assignmentAttempt = AssignStartNodesToUserGroup(requestModel, group);
        if (assignmentAttempt.Success is false)
        {
            return Attempt.FailWithStatus<IUserGroup, UserGroupOperationStatus>(assignmentAttempt.Result, group);
        }

        foreach (var section in requestModel.Sections)
        {
            group.AddAllowedSection(SectionMapper.GetAlias(section));
        }

        Attempt<IEnumerable<int>, UserGroupOperationStatus> languageIsoCodeMappingAttempt = await MapLanguageIsoCodesToIdsAsync(requestModel.Languages);
        if (languageIsoCodeMappingAttempt.Success is false)
        {
            return Attempt.FailWithStatus<IUserGroup, UserGroupOperationStatus>(languageIsoCodeMappingAttempt.Status, group);
        }

        foreach (var languageId in languageIsoCodeMappingAttempt.Result)
        {
            group.AddAllowedLanguage(languageId);
        }

        return Attempt.SucceedWithStatus<IUserGroup, UserGroupOperationStatus>(UserGroupOperationStatus.Success, group);
    }

    /// <inheritdoc />
    public async Task<Attempt<IUserGroup, UserGroupOperationStatus>> UpdateAsync(IUserGroup current, UpdateUserGroupRequestModel request)
    {
        Attempt<UserGroupOperationStatus> assignmentAttempt = AssignStartNodesToUserGroup(request, current);
        if (assignmentAttempt.Success is false)
        {
            return Attempt.FailWithStatus(assignmentAttempt.Result, current);
        }

        current.ClearAllowedLanguages();
        Attempt<IEnumerable<int>, UserGroupOperationStatus> languageIdsMappingAttempt = await MapLanguageIsoCodesToIdsAsync(request.Languages);
        if (languageIdsMappingAttempt.Success is false)
        {
            return Attempt.FailWithStatus(languageIdsMappingAttempt.Status, current);
        }

        foreach (var languageId in languageIdsMappingAttempt.Result)
        {
            current.AddAllowedLanguage(languageId);
        }

        current.ClearAllowedSections();
        foreach (var sectionName in request.Sections)
        {
            current.AddAllowedSection(SectionMapper.GetAlias(sectionName));
        }

        current.Name = request.Name.CleanForXss('[', ']', '(', ')', ':');
        current.Icon = request.Icon;
        current.HasAccessToAllLanguages = request.HasAccessToAllLanguages;
        current.PermissionNames = request.Permissions;


        return Attempt.SucceedWithStatus(UserGroupOperationStatus.Success, current);
    }

    private async Task<Attempt<IEnumerable<string>, UserGroupOperationStatus>> MapLanguageIdsToIsoCodeAsync(IEnumerable<int> ids)
    {
        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();
        string[] isoCodes = languages
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.IsoCode)
            .ToArray();

        return isoCodes.Length == ids.Count()
            ? Attempt.SucceedWithStatus<IEnumerable<string>, UserGroupOperationStatus>(UserGroupOperationStatus.Success, isoCodes)
            : Attempt.FailWithStatus<IEnumerable<string>, UserGroupOperationStatus>(UserGroupOperationStatus.LanguageNotFound, isoCodes);
    }

    private async Task<Attempt<IEnumerable<int>, UserGroupOperationStatus>> MapLanguageIsoCodesToIdsAsync(IEnumerable<string> isoCodes)
    {
        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();
        int[] languageIds = languages
            .Where(x => isoCodes.Contains(x.IsoCode))
            .Select(x => x.Id)
            .ToArray();

        return languageIds.Length == isoCodes.Count()
            ? Attempt.SucceedWithStatus<IEnumerable<int>, UserGroupOperationStatus>(UserGroupOperationStatus.Success, languageIds)
            : Attempt.FailWithStatus<IEnumerable<int>, UserGroupOperationStatus>(UserGroupOperationStatus.LanguageNotFound, languageIds);
    }

    private Attempt<UserGroupOperationStatus> AssignStartNodesToUserGroup(UserGroupBase source, IUserGroup target)
    {
        if (source.DocumentStartNodeId is not null)
        {
            var contentId = GetIdFromKey(source.DocumentStartNodeId.Value, UmbracoObjectTypes.Document);

            if (contentId is null)
            {
                return Attempt.Fail(UserGroupOperationStatus.DocumentStartNodeKeyNotFound);
            }

            target.StartContentId = contentId;
        }
        else
        {
            target.StartContentId = null;
        }

        if (source.MediaStartNodeId is not null)
        {
            var mediaId = GetIdFromKey(source.MediaStartNodeId.Value, UmbracoObjectTypes.Media);

            if (mediaId is null)
            {
                return Attempt.Fail(UserGroupOperationStatus.MediaStartNodeKeyNotFound);
            }

            target.StartMediaId = mediaId;
        }
        else
        {
            target.StartMediaId = null;
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
