using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(DistributedJobConfiguration))]
public class DistributedJobDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DistributedJob;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    /// <summary>
    /// Gets or sets the unique identifier for the distributed job.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the distributed job.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the job was last run.
    /// </summary>
    public DateTime LastRun { get; set; }

    /// <summary>
    /// Gets or sets the interval period for the distributed job, in milliseconds.
    /// </summary>
    public long Period { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the distributed job is currently running.
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the job was last attempted to run.
    /// </summary>
    public DateTime LastAttemptedRun { get; set; }
}
