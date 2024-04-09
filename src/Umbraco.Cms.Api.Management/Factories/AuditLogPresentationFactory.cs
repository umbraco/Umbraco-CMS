using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.AuditLogs;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class AuditLogPresentationFactory : IAuditLogPresentationFactory
{
    private readonly IUserService _userService;
    private readonly AppCaches _appCaches;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IImageUrlGenerator _imageUrlGenerator;
    private readonly IEntityService _entityService;

    public AuditLogPresentationFactory(IUserService userService, AppCaches appCaches, MediaFileManager mediaFileManager, IImageUrlGenerator imageUrlGenerator, IEntityService entityService)
    {
        _userService = userService;
        _appCaches = appCaches;
        _mediaFileManager = mediaFileManager;
        _imageUrlGenerator = imageUrlGenerator;
        _entityService = entityService;
    }

    public IEnumerable<AuditLogResponseModel> CreateAuditLogViewModel(IEnumerable<IAuditItem> auditItems) => auditItems.Select(CreateAuditLogViewModel);

    public IEnumerable<AuditLogWithUsernameResponseModel> CreateAuditLogWithUsernameViewModels(IEnumerable<IAuditItem> auditItems) => auditItems.Select(CreateAuditLogWithUsernameViewModel);

    private AuditLogWithUsernameResponseModel CreateAuditLogWithUsernameViewModel(IAuditItem auditItem)
    {
        AuditLogWithUsernameResponseModel target = CreateResponseModel<AuditLogWithUsernameResponseModel>(auditItem, out IUser user);

        target.UserAvatars = user.GetUserAvatarUrls(_appCaches.RuntimeCache, _mediaFileManager, _imageUrlGenerator);
        target.UserName = user.Name;
        return target;
    }

    private AuditLogResponseModel CreateAuditLogViewModel(IAuditItem auditItem)
        => CreateResponseModel<AuditLogResponseModel>(auditItem, out _);

    private T CreateResponseModel<T>(IAuditItem auditItem, out IUser user)
        where T : AuditLogBaseModel, new()
    {
        user = _userService.GetUserById(auditItem.UserId)
               ?? throw new ArgumentException($"Could not find user with id {auditItem.UserId}");

        IEntitySlim? entitySlim = _entityService.Get(auditItem.Id);

        return new T
        {
            Comment = auditItem.Comment,
            Entity = auditItem.EntityType is not null || entitySlim is not null
                ? new AuditLogEntity
                {
                    Id = entitySlim?.Key,
                    Type = auditItem.EntityType
                }
                : null,
            LogType = auditItem.AuditType,
            Parameters = auditItem.Parameters,
            Timestamp = auditItem.CreateDate,
            User = new ReferenceByIdModel(user.Key)
        };
    }
}
