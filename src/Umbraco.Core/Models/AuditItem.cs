using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents an audit log entry.
/// </summary>
public sealed class AuditItem : EntityBase, IAuditItem
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AuditItem" /> class.
    /// </summary>
    public AuditItem(
        int objectId,
        AuditType type,
        int userId,
        string? entityType,
        string? comment = null,
        string? parameters = null,
        DateTime? createDate = null,
        string? triggerSource = null,
        string? triggerOperation = null,
        string? typeAlias = null)
    {
        DisableChangeTracking();

        Id = objectId;
        Comment = comment;
        AuditType = type;
        UserId = userId;
        EntityType = entityType;
        Parameters = parameters;
        CreateDate = createDate ?? default;
        TriggerSource = triggerSource;
        TriggerOperation = triggerOperation;
        TypeAlias = typeAlias;

        EnableChangeTracking();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuditItem" /> class.
    /// </summary>
    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public AuditItem(int objectId, AuditType type, int userId, string? entityType, string? comment, string? parameters, DateTime? createDate)
        : this(objectId, type, userId, entityType, comment, parameters, createDate, triggerSource: null, triggerOperation: null, typeAlias: null)
    {
    }

    /// <inheritdoc />
    public AuditType AuditType { get; }

    /// <inheritdoc />
    public string? EntityType { get; }

    /// <inheritdoc />
    public int UserId { get; }

    /// <inheritdoc />
    public string? Comment { get; }

    /// <inheritdoc />
    public string? Parameters { get; }

    /// <inheritdoc />
    public string? TriggerSource { get; }

    /// <inheritdoc />
    public string? TriggerOperation { get; }

    /// <inheritdoc />
    public string? TypeAlias { get; }
}
