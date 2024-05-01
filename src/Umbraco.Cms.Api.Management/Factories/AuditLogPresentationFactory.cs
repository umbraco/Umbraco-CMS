using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.AuditLog;
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

    private AuditLogResponseModel CreateAuditLogViewModel(IAuditItem auditItem)
    {
        Guid userKey = _userIdKeyResolver.GetAsync(auditItem.UserId).GetAwaiter().GetResult();
        IUser user = _userService.GetAsync(userKey).GetAwaiter().GetResult()
                     ?? throw new ArgumentException($"Could not find user with id {auditItem.UserId}");

        return new AuditLogResponseModel
        {
            Comment = auditItem.Comment,
            LogType = auditItem.AuditType,
            Parameters = auditItem.Parameters,
            Timestamp = auditItem.CreateDate,
            User = new ReferenceByIdModel(user.Key)
        };
    }
}
