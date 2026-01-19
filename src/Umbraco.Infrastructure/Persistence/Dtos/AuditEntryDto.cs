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

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    // there is NO foreign key to the users table here, neither for performing user nor for
    // affected user, so we can delete users and NOT delete the associated audit trails, and
    // users can still be identified via the details free-form text fields.
    [Column("performingUserId")]
    public int PerformingUserId { get; set; }

    [Column("performingUserKey")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public Guid? PerformingUserKey { get; set; }

    [Column("performingDetails")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(Constants.Audit.DetailsLength)]
    public string? PerformingDetails { get; set; }

    [Column("performingIp")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(Constants.Audit.IpLength)]
    public string? PerformingIp { get; set; }

    [Column("eventDateUtc")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime EventDate { get; set; }

    [Column("affectedUserId")]
    public int AffectedUserId { get; set; }

    [Column("affectedUserKey")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public Guid? AffectedUserKey { get; set; }

    [Column("affectedDetails")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(Constants.Audit.DetailsLength)]
    public string? AffectedDetails { get; set; }

    [Column("eventType")]
    [Length(Constants.Audit.EventTypeLength)]
    public string? EventType { get; set; }

    [Column("eventDetails")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(Constants.Audit.DetailsLength)]
    public string? EventDetails { get; set; }
}
