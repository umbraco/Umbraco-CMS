using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.AuditLogs;

public class AuditLogWithUsernameViewModel
{
    public Guid UserKey { get; set; }

    public string? UserName { get; set; }

    public string[]? UserAvatars { get; set; }

    public DateTime Timestamp { get; set; }

    public AuditType LogType { get; set; }

    public string? EntityType { get; set; }

    public string? Comment { get; set; }

    public string? Parameters { get; set; }
}
