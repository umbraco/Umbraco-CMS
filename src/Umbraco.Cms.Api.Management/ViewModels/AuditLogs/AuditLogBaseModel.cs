using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.AuditLogs;

public class AuditLogBaseModel
{
    public ReferenceByIdModel User { get; set; } = new();

    public AuditLogEntity? Entity { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public AuditType LogType { get; set; }

    public string? Comment { get; set; }

    public string? Parameters { get; set; }
}
