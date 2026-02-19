using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class ServerRegistrationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Server;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    [Column("address")]
    [Length(500)]
    public string? ServerAddress { get; set; }

    [Column("computerName")]
    [Length(255)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_computerName")] // server identity is unique
    public string? ServerIdentity { get; set; }

    [Column("registeredDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime DateRegistered { get; set; }

    [Column("lastNotifiedDate")]
    public DateTime DateAccessed { get; set; }

    [Column("isActive")]
    [Index(IndexTypes.NonClustered)]
    public bool IsActive { get; set; }

    [Column("isSchedulingPublisher")]
    public bool IsSchedulingPublisher { get; set; }
}
