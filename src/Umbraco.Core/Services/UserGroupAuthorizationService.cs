using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public class UserGroupAuthorizationService : IUserGroupAuthorizationService
{
    private readonly IContentService _contentService;
    private readonly IMediaService _mediaService;
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;

    public UserGroupAuthorizationService(
        IContentService contentService,
        IMediaService mediaService,
        IEntityService entityService,
        AppCaches appCaches)
    {
        _contentService = contentService;
        _mediaService = mediaService;
        _entityService = entityService;
        _appCaches = appCaches;
    }

    /// <inheritdoc />
    public Attempt<UserGroupOperationStatus> AuthorizeSectionAccess(IUser performingUser, IUserGroup userGroup)
    {
        if (performingUser.IsAdmin())
        {
            return Attempt.Succeed(UserGroupOperationStatus.Success);
        }

        IEnumerable<string> sectionsMissingAccess = userGroup.AllowedSections.Except(performingUser.AllowedSections).ToArray();
        return sectionsMissingAccess.Any()
            ? Attempt.Fail(UserGroupOperationStatus.UnauthorizedMissingSections)
            : Attempt.Succeed(UserGroupOperationStatus.Success);
    }

    /// <inheritdoc />
    public Attempt<UserGroupOperationStatus> AuthorizeStartNodeChanges(IUser user, IUserGroup userGroup)
    {
        Attempt<UserGroupOperationStatus> authorizeContent = AuthorizeContentStartNode(user, userGroup);

        return authorizeContent.Success is false
            ? authorizeContent
            : AuthorizeMediaStartNode(user, userGroup);
    }

    /// <inheritdoc />
    public Attempt<UserGroupOperationStatus> HasAccessToUserSection(IUser user)
    {
        if (user.AllowedSections.Contains(Constants.Applications.Users) is false)
        {
            return Attempt.Fail(UserGroupOperationStatus.UnauthorizedMissingUserSection);
        }

        return Attempt.Succeed(UserGroupOperationStatus.Success);
    }

    public Attempt<UserGroupOperationStatus> AuthorizeUserGroupCreation(IUser performingUser, IUserGroup userGroup)
    {
        Attempt<UserGroupOperationStatus> hasSectionAccess = HasAccessToUserSection(performingUser);
        if (hasSectionAccess.Success is false)
        {
            return Attempt.Fail(hasSectionAccess.Result);
        }

        Attempt<UserGroupOperationStatus> authorizeSectionChanges = AuthorizeSectionAccess(performingUser, userGroup);
        if (authorizeSectionChanges.Success is false)
        {
            return Attempt.Fail(authorizeSectionChanges.Result);
        }

        Attempt<UserGroupOperationStatus> authorizeContentNodeChanges = AuthorizeStartNodeChanges(performingUser, userGroup);
        return authorizeSectionChanges.Success is false
            ? Attempt.Fail(authorizeContentNodeChanges.Result)
            : Attempt.Succeed(UserGroupOperationStatus.Success);
    }

    public Attempt<UserGroupOperationStatus> AuthorizeUserGroupUpdate(IUser performingUser, IUserGroup userGroup) => throw new NotImplementedException();

    // We explicitly take an IUser here which is non-nullable, since nullability should be handled in caller.
    private Attempt<UserGroupOperationStatus> AuthorizeContentStartNode(IUser user, IUserGroup userGroup)
    {
        if (userGroup.StartContentId is null)
        {
            return Attempt.Succeed(UserGroupOperationStatus.Success);
        }

        IContent? content = _contentService.GetById(userGroup.StartContentId.Value);

        if (content is null)
        {
            return Attempt.Succeed(UserGroupOperationStatus.Success);
        }

        return user.HasPathAccess(content, _entityService, _appCaches) is false
            ? Attempt.Fail(UserGroupOperationStatus.UnauthorizedStartNodes)
            : Attempt.Succeed(UserGroupOperationStatus.Success);
    }

    // We explicitly take an IUser here which is non-nullable, since nullability should be handled in caller.
    private Attempt<UserGroupOperationStatus> AuthorizeMediaStartNode(IUser user, IUserGroup userGroup)
    {

        if (userGroup.StartMediaId is null)
        {
            return Attempt.Succeed(UserGroupOperationStatus.Success);
        }

        IMedia? media = _mediaService.GetById(userGroup.StartMediaId.Value);

        if (media is null)
        {
            return Attempt.Succeed(UserGroupOperationStatus.Success);
        }

        return user.HasPathAccess(media, _entityService, _appCaches) is false
            ? Attempt.Fail(UserGroupOperationStatus.UnauthorizedStartNodes)
            : Attempt.Succeed(UserGroupOperationStatus.Success);
    }
}
