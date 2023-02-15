using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.AuditLogs;

public class AuditLogByTypeViewModel
{
    public Guid UserKey { get; set; }

    public DateTime Timestamp { get; set; }

    public AuditType LogType { get; set; }

    public string? EntityType { get; set; }

    public string? Comment { get; set; }

    public string? Parameters { get; set; }
}
