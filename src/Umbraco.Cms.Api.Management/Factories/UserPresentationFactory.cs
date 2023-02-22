using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class UserPresentationFactory : IUserPresentationFactory
{
    private readonly IEntityService _entityService;

    public UserPresentationFactory(IEntityService entityService)
    {
        _entityService = entityService;
    }

    public UserResponseModel CreateResponseModel(IUser user)
    {
        var responseModel = new UserResponseModel
        {
            Key = user.Key,
            Email = user.Email,
            Name = user.Name ?? string.Empty,
            UserName = user.Username,
            Language = user.Language,
            CreateDate = user.CreateDate,
            UpdateDate = user.UpdateDate,
            State = user.UserState,
            UserGroups = new SortedSet<Guid>(user.Groups.Select(x => x.Key)),
            ContentStartNodeKeys = GetKeysFromIds(user.StartContentIds, UmbracoObjectTypes.Document),
            MediaStartNodeKeys = GetKeysFromIds(user.StartMediaIds, UmbracoObjectTypes.Media),
            FailedLoginAttempts = user.FailedPasswordAttempts,
            LastLoginDate = user.LastLoginDate,
            LastlockoutDate = user.LastLockoutDate,
            LastPasswordChangeDate = user.LastPasswordChangeDate,
        };

        return responseModel;
    }

    private SortedSet<Guid> GetKeysFromIds(IEnumerable<int>? ids, UmbracoObjectTypes type)
    {
        IEnumerable<Guid>? keys = ids?
            .Select(x => _entityService.GetKey(x, type))
            .Where(x => x.Success)
            .Select(x => x.Result);

        return keys is null
            ? new SortedSet<Guid>()
            : new SortedSet<Guid>(keys);
    }
}
