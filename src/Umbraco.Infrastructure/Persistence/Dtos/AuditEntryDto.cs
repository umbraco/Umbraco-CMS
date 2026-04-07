using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class AuditEntryDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.AuditEntry;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    /// <summary>
    /// Gets or sets the unique identifier for the audit entry.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who performed the action.
    /// </summary>
    /// <remarks>
    /// there is NO foreign key to the users table here, neither for performing user nor for
    /// affected user, so we can delete users and NOT delete the associated audit trails, and
    /// users can still be identified via the details free-form text fields.
    /// </remarks>
    [Column("performingUserId")]
    public int PerformingUserId { get; set; }

    /// <summary>
    /// Gets or sets the unique key (GUID) of the user who performed the action associated with this audit entry.
    /// </summary>
    [Column("performingUserKey")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public Guid? PerformingUserKey { get; set; }

    /// <summary>
    /// Gets or sets additional details about the user or process that performed the audited action.
    /// </summary>
    [Column("performingDetails")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(Constants.Audit.DetailsLength)]
    public string? PerformingDetails { get; set; }

    /// <summary>
    /// Gets or sets the IP address from which the action was performed.
    /// </summary>
    [Column("performingIp")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(Constants.Audit.IpLength)]
    public string? PerformingIp { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the event occurred in UTC.
    /// </summary>
    [Column("eventDateUtc")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime EventDate { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user affected by the audit entry.
    /// </summary>
    [Column("affectedUserId")]
    public int AffectedUserId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (key) of the user affected by the audit entry, if applicable.
    /// </summary>
    [Column("affectedUserKey")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public Guid? AffectedUserKey { get; set; }

    /// <summary>
    /// Gets or sets additional details about what was affected in this audit entry.
    /// </summary>
    [Column("affectedDetails")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(Constants.Audit.DetailsLength)]
    public string? AffectedDetails { get; set; }

    /// <summary>
    /// Gets or sets the string that represents the type of the audit event associated with this entry.
    /// </summary>
    [Column("eventType")]
    [Length(Constants.Audit.EventTypeLength)]
    public string? EventType { get; set; }

    /// <summary>
    /// Gets or sets additional details or information about the audit event. This value may be <c>null</c>.
    /// </summary>
    [Column("eventDetails")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(Constants.Audit.DetailsLength)]
    public string? EventDetails { get; set; }
}
