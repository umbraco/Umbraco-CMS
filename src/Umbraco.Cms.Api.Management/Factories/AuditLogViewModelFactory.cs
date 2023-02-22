using Umbraco.Cms.Api.Management.ViewModels.AuditLogs;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class AuditLogViewModelFactory : IAuditLogViewModelFactory
{
    private readonly IUserService _userService;
    private readonly AppCaches _appCaches;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IImageUrlGenerator _imageUrlGenerator;
    private readonly IEntityService _entityService;

    public AuditLogViewModelFactory(IUserService userService, AppCaches appCaches, MediaFileManager mediaFileManager, IImageUrlGenerator imageUrlGenerator, IEntityService entityService)
    {
        _userService = userService;
        _appCaches = appCaches;
        _mediaFileManager = mediaFileManager;
        _imageUrlGenerator = imageUrlGenerator;
        _entityService = entityService;
    }

    public IEnumerable<AuditlogViewModel> CreateAuditLogViewModel(IEnumerable<IAuditItem> auditItems) => auditItems.Select(CreateAuditLogViewModel);

    public IEnumerable<AuditLogWithUsernameViewModel> CreateAuditLogWithUsernameViewModels(IEnumerable<IAuditItem> auditItems) => auditItems.Select(CreateAuditLogWithUsernameViewModel);

    private AuditLogWithUsernameViewModel CreateAuditLogWithUsernameViewModel(IAuditItem auditItem)
    {
        var target = new AuditLogWithUsernameViewModel
        {
            Comment = auditItem.Comment,
            EntityType = auditItem.EntityType,
            LogType = auditItem.AuditType,
            Parameters = auditItem.Parameters,
            Timestamp = auditItem.CreateDate,
        };

        IUser? user = _userService.GetUserById(auditItem.UserId);
        if (user is null)
        {
            throw new ArgumentException($"Could not find user with id {auditItem.UserId}");
        }

        target.UserKey = user.Key;
        target.UserAvatars = user.GetUserAvatarUrls(_appCaches.RuntimeCache, _mediaFileManager, _imageUrlGenerator);
        target.UserName = user.Name;
        return target;
    }

    private AuditlogViewModel CreateAuditLogViewModel(IAuditItem auditItem)
    {
        IEntitySlim? entitySlim = _entityService.Get(auditItem.Id);
        var target = new AuditlogViewModel
            {
                Comment = auditItem.Comment,
                EntityType = auditItem.EntityType,
                EntityKey = entitySlim?.Key,
                LogType = auditItem.AuditType,
                Parameters = auditItem.Parameters,
                Timestamp = auditItem.CreateDate,
            };

        IUser? user = _userService.GetUserById(auditItem.UserId);
        if (user is null)
        {
            throw new ArgumentException($"Could not find user with id {auditItem.UserId}");
        }

        target.UserKey = user.Key;
        return target;
    }
}
