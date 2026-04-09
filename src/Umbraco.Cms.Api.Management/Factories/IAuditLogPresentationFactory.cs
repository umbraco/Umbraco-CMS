using Umbraco.Cms.Api.Management.ViewModels.AuditLog;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory interface for creating presentation models of audit logs.
/// </summary>
public interface IAuditLogPresentationFactory
{
    /// <summary>
    /// Creates a collection of <see cref="AuditLogResponseModel"/> view models from the given audit items.
    /// </summary>
    /// <param name="auditItems">The collection of audit items to convert.</param>
    /// <returns>An enumerable of <see cref="AuditLogResponseModel"/> representing the audit log entries.</returns>
    IEnumerable<AuditLogResponseModel> CreateAuditLogViewModel(IEnumerable<IAuditItem> auditItems);
}
