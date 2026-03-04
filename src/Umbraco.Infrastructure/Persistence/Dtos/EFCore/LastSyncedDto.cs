using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(LastSyncedDtoConfiguration))]
public class LastSyncedDto
{
    internal const string TableName = Constants.DatabaseSchema.Tables.LastSynced;
    public const string PrimaryKeyColumnName = "machineId";

    public required string MachineId { get; set; }

    public int? LastSyncedInternalId { get; set; }

    public int? LastSyncedExternalId { get; set; }

    public DateTime LastSyncedDate { get; set; }
}
