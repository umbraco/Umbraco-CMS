using Umbraco.Cms.Api.Management.ViewModels.AuditLogs;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class AuditLogViewModelFactory : IAuditLogViewModelFactory
{
    private readonly IUserService _userService;

    public AuditLogViewModelFactory(IUserService userService)
    {
        _userService = userService;
    }

    public IEnumerable<AuditLogByTypeViewModel> CreateAuditLogViewModel(IEnumerable<IAuditItem> auditItems) => auditItems.Select(CreateAuditLogViewModel);

    private AuditLogByTypeViewModel CreateAuditLogViewModel(IAuditItem auditItem)
    {
        var target = new AuditLogByTypeViewModel
            {
                Comment = auditItem.Comment, EntityType = auditItem.EntityType, LogType = auditItem.AuditType,
                Parameters = auditItem.Parameters,
                Timestamp = auditItem.CreateDate,
            };

        IUser? user = _userService.GetUserById(auditItem.UserId);
        if (user is null)
        {
            throw new ArgumentException($"Could not find user with id {auditItem.UserId}");
        }

        target.UserKey = _userService.GetUserById(auditItem.UserId)!.Key;
        return target;
    }
}
