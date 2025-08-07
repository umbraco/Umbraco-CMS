using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;


[TableName(TableName)]
[ExplicitColumns]
public class LastSyncedDto
{
    internal const string TableName = Constants.DatabaseSchema.Tables.LastSynced;

    [Column("MachineId")]
    public required string MachineId { get; set; }

    [Column("LastSyncedInternalId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? LastSyncedInternalId { get; set; }

    [Column("LastSyncedExternalId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? LastSyncedExternalId { get; set; }

    [Column("LastSyncedDate")]
    public DateTime LastSyncedDate { get; set; }
}
