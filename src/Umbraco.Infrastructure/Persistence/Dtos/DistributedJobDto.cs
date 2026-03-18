using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = true)]
[ExplicitColumns]
internal sealed class DistributedJobDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DistributedJob;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    /// <summary>
    /// Gets or sets the unique identifier for the distributed job.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the distributed job.
    /// </summary>
    [Column("Name")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the job was last run.
    /// </summary>
    [Column("lastRun")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime LastRun { get; set; }

    /// <summary>
    /// Gets or sets the interval period for the distributed job, in milliseconds.
    /// </summary>
    [Column("period")]
    public long Period { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the distributed job is currently running.
    /// </summary>
    [Column("IsRunning")]
    public bool IsRunning { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the job was last attempted to run.
    /// </summary>
    [Column("lastAttemptedRun")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime LastAttemptedRun { get; set; }
}
