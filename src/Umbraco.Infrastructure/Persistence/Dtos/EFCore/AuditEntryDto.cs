using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

/// <summary>
/// Data transfer object representing an audit entry in the Umbraco CMS persistence layer.
/// </summary>
/// <remarks>
/// There is intentionally no foreign key to the users table, neither for performing user nor for
/// affected user, so users can be deleted while their associated audit trails remain. The users
/// can still be identified via the *UserKey columns and the free-form *Details text fields.
/// </remarks>
[EntityTypeConfiguration(typeof(AuditEntryDtoConfiguration))]
public class AuditEntryDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.AuditEntry;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    // Public constants to bind properties between configurations and consumers.
    public const string PerformingUserIdColumnName = "performingUserId";
    public const string PerformingUserKeyColumnName = "performingUserKey";
    public const string PerformingDetailsColumnName = "performingDetails";
    public const string PerformingIpColumnName = "performingIp";
    public const string EventDateColumnName = "eventDateUtc";
    public const string AffectedUserIdColumnName = "affectedUserId";
    public const string AffectedUserKeyColumnName = "affectedUserKey";
    public const string AffectedDetailsColumnName = "affectedDetails";
    public const string EventTypeColumnName = "eventType";
    public const string EventDetailsColumnName = "eventDetails";

    /// <summary>
    /// Gets or sets the unique identifier for the audit entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who performed the action.
    /// </summary>
    public int PerformingUserId { get; set; }

    /// <summary>
    /// Gets or sets the unique key (GUID) of the user who performed the action.
    /// </summary>
    public Guid? PerformingUserKey { get; set; }

    /// <summary>
    /// Gets or sets free-form details about the user or process that performed the audited action.
    /// </summary>
    public string? PerformingDetails { get; set; }

    /// <summary>
    /// Gets or sets the IP address from which the action was performed.
    /// </summary>
    public string? PerformingIp { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the event occurred in UTC.
    /// </summary>
    public DateTime EventDate { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user affected by the audit entry.
    /// </summary>
    public int AffectedUserId { get; set; }

    /// <summary>
    /// Gets or sets the unique key (GUID) of the user affected by the audit entry, if applicable.
    /// </summary>
    public Guid? AffectedUserKey { get; set; }

    /// <summary>
    /// Gets or sets free-form details about what was affected in this audit entry.
    /// </summary>
    public string? AffectedDetails { get; set; }

    /// <summary>
    /// Gets or sets the type of audit event associated with this entry.
    /// </summary>
    public string? EventType { get; set; }

    /// <summary>
    /// Gets or sets free-form details about the audit event.
    /// </summary>
    public string? EventDetails { get; set; }
}
