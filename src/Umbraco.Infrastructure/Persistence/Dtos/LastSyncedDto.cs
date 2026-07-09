using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;


/// <summary>
/// Data Transfer Object (DTO) representing information about the last synchronization event for a data entity.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
public class LastSyncedDto
{
    internal const string TableName = Constants.DatabaseSchema.Tables.LastSynced;
    public const string PrimaryKeyColumnName = "machineId";

    /// <summary>
    /// Gets or sets the unique identifier of the machine that performed the last synchronization.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_lastSyncedMachineId", AutoIncrement = false, Clustered = true)]
    public required string MachineId { get; set; }

    /// <summary>
    /// Gets or sets the nullable internal identifier of the last synchronized item.
    /// </summary>
    [Column("lastSyncedInternalId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? LastSyncedInternalId { get; set; }

    /// <summary>
    /// Gets or sets the nullable external identifier associated with the last synchronized entity.
    /// </summary>
    [Column("lastSyncedExternalId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? LastSyncedExternalId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the last synchronization occurred.
    /// </summary>
    [Column("lastSyncedDate")]
    public DateTime LastSyncedDate { get; set; }
}
