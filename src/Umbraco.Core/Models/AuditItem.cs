using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public sealed class AuditItem : EntityBase, IAuditItem
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AuditItem" /> class.
    /// </summary>
    public AuditItem(int objectId, AuditType type, int userId, string? entityType, string? comment = null, string? parameters = null)
    {
        DisableChangeTracking();

        Id = objectId;
        Comment = comment;
        AuditType = type;
        UserId = userId;
        EntityType = entityType;
        Parameters = parameters;

        EnableChangeTracking();
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
}
