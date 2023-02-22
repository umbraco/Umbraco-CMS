using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.AuditLogs;

public class AuditLogBaseModel
{
    public Guid UserKey { get; set; }

    public Guid? EntityKey { get; set; }

    public DateTime Timestamp { get; set; }

    public AuditType LogType { get; set; }

    public string? EntityType { get; set; }

    public string? Comment { get; set; }

    public string? Parameters { get; set; }
}
