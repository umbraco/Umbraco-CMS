using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.Server)]
[PrimaryKey("id")]
[ExplicitColumns]
internal sealed class ServerRegistrationDto
{
    [Column("id")]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    [Column("address")]
    [Length(500)]
    public string? ServerAddress { get; set; }

    [Column("computerName")]
    [Length(255)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_computerName")] // server identity is unique
    public string? ServerIdentity { get; set; }

    [Column("registeredDate", ForceToUtc = false)]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime DateRegistered { get; set; }

    [Column("lastNotifiedDate", ForceToUtc = false)]
    public DateTime DateAccessed { get; set; }

    [Column("isActive")]
    [Index(IndexTypes.NonClustered)]
    public bool IsActive { get; set; }

    [Column("isSchedulingPublisher")]
    public bool IsSchedulingPublisher { get; set; }
}
