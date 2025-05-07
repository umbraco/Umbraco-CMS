using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.AuditLog;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class AuditLogPresentationFactory : IAuditLogPresentationFactory
{
    private readonly IUserService _userService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    public AuditLogPresentationFactory(IUserService userService, IUserIdKeyResolver userIdKeyResolver)
    {
        _userService = userService;
        _userIdKeyResolver = userIdKeyResolver;
    }

    public IEnumerable<AuditLogResponseModel> CreateAuditLogViewModel(IEnumerable<IAuditItem> auditItems) => auditItems.Select(CreateAuditLogViewModel);

    private AuditLogResponseModel CreateAuditLogViewModel(IAuditItem auditItem) =>
        new()
        {
            Comment = auditItem.Comment,
            LogType = auditItem.AuditType,
            Parameters = auditItem.Parameters,
            Timestamp = auditItem.CreateDate,
            User = CreateUserReference(auditItem.UserId).GetAwaiter().GetResult(),
        };

    private async Task<ReferenceByIdModel> CreateUserReference(int userId)
    {
        if (userId == Constants.Security.UnknownUserId)
        {
            return new ReferenceByIdModel();
        }

        Guid userKey = await _userIdKeyResolver.GetAsync(userId);
        IUser user = await _userService.GetAsync(userKey)
                     ?? throw new ArgumentException($"Could not find user with id {userId}");
        return new ReferenceByIdModel(user.Key);
    }
}
