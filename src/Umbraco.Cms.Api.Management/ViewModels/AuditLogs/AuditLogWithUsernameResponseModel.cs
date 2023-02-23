using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.AuditLogs;

public class AuditLogWithUsernameResponseModel : AuditLogBaseModel
{
    public string? UserName { get; set; }

    public string[]? UserAvatars { get; set; }
}
