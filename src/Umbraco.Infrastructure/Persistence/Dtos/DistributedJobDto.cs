using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = true)]
[ExplicitColumns]
internal sealed class DistributedJobDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DistributedJob;
    public const string PrimaryKeyName = Constants.DatabaseSchema.PrimaryKeyNameId;

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    [Column("Name")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public required string Name { get; set; }

    [Column("lastRun")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime LastRun { get; set; }

    [Column("period")]
    public long Period { get; set; }

    [Column("IsRunning")]
    public bool IsRunning { get; set; }

    [Column("lastAttemptedRun")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime LastAttemptedRun { get; set; }
}
