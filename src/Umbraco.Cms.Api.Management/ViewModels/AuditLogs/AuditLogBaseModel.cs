using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.AuditLogs;

public class AuditLogBaseModel
{
    public Guid UserId { get; set; }

    public Guid? EntityId { get; set; }

    public DateTime Timestamp { get; set; }

    public AuditType LogType { get; set; }

    public string? EntityType { get; set; }

    public string? Comment { get; set; }

    public string? Parameters { get; set; }
}
