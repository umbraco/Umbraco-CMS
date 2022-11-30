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
}
