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

    /// <summary>
    /// Gets or sets the unique identifier for the server registration.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the server address.
    /// </summary>
    [Column("address")]
    [Length(500)]
    public string? ServerAddress { get; set; }

    /// <summary>
    /// Gets or sets the unique server identity, typically the computer name.
    /// </summary>
    /// <remarks>server identity is unique</remarks>
    [Column("computerName")]
    [Length(255)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_computerName")]
    public string? ServerIdentity { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the server was registered.
    /// </summary>
    [Column("registeredDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime DateRegistered { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the server was last accessed.
    /// </summary>
    [Column("lastNotifiedDate")]
    public DateTime DateAccessed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the server registration is active.
    /// </summary>
    [Column("isActive")]
    [Index(IndexTypes.NonClustered)]
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this server instance is designated as the scheduling publisher in a load-balanced environment.
    /// </summary>
    [Column("isSchedulingPublisher")]
    public bool IsSchedulingPublisher { get; set; }
}
