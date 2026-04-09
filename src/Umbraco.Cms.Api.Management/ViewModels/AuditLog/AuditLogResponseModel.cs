using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.AuditLog;

/// <summary>
/// Represents a model containing information about a single audit log entry returned in API responses.
/// </summary>
public class AuditLogResponseModel
{
    /// <summary>
    /// Gets or sets the user associated with the audit log entry.
    /// </summary>
    public ReferenceByIdModel User { get; set; } = new();

    /// <summary>
    /// Gets or sets the timestamp of the audit log entry.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the type of the audit log entry.
    /// </summary>
    public AuditType LogType { get; set; }

    /// <summary>Gets or sets the comment associated with the audit log entry.</summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the parameters associated with the audit log entry.
    /// </summary>
    public string? Parameters { get; set; }
}
