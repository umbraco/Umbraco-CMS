using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.AuditLog;

public class AuditLogResponseModel
{
    public ReferenceByIdModel User { get; set; } = new();

    public DateTimeOffset Timestamp { get; set; }

    public AuditType LogType { get; set; }

    public string? Comment { get; set; }

    public string? Parameters { get; set; }
}
