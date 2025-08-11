using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;


[TableName(TableName)]
[PrimaryKey("machineId", AutoIncrement = false)]
[ExplicitColumns]
public class LastSyncedDto
{
    internal const string TableName = Constants.DatabaseSchema.Tables.LastSynced;

    [Column("machineId")]
    [PrimaryKeyColumn(Name = "PK_lastSyncedMachineId", AutoIncrement = false, Clustered = true)]
    public required string MachineId { get; set; }

    [Column("lastSyncedInternalId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? LastSyncedInternalId { get; set; }

    [Column("lastSyncedExternalId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? LastSyncedExternalId { get; set; }

    [Column("lastSyncedDate")]
    public DateTime LastSyncedDate { get; set; }
}
