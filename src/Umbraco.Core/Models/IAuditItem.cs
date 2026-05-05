using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents an audit item.
/// </summary>
public interface IAuditItem : IEntity
{
    /// <summary>
    ///     Gets the audit type.
    /// </summary>
    AuditType AuditType { get; }

    /// <summary>
    ///     Gets the audited entity type.
    /// </summary>
    string? EntityType { get; }

    /// <summary>
    ///     Gets the audit user identifier.
    /// </summary>
    int UserId { get; }

    /// <summary>
    ///     Gets the audit comments.
    /// </summary>
    string? Comment { get; }

    /// <summary>
    ///     Gets optional additional data parameters.
    /// </summary>
    string? Parameters { get; }

    // TODO (V18): Remove the default implementations.

    /// <summary>
    ///     Gets the source of the trigger that initiated the audited action (e.g. "Core", "Umbraco.Workflow"),
    ///     or <c>null</c> if no trigger context was set.
    /// </summary>
    string? TriggerSource => null;

    /// <summary>
    ///     Gets the operation of the trigger that initiated the audited action (e.g. "ScheduledPublish", "Rollback", "FinalApproval"),
    ///     or <c>null</c> if no trigger context was set.
    /// </summary>
    string? TriggerOperation => null;

    /// <summary>
    ///     Gets the type alias for custom audit entries (e.g. "Umb.Workflow.Approved"),
    ///     or <c>null</c> for built-in audit types.
    /// </summary>
    string? TypeAlias => null;
}
