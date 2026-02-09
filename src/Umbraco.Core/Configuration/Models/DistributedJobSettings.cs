using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Settings for distributed jobs.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigDistributedJobs)]
public class DistributedJobSettings
{
    /// <summary>
    ///     The default period for checking if there are runnable distributed jobs.
    /// </summary>
    internal const string StaticPeriod = "00:00:05";

    /// <summary>
    ///     The default delay before starting to check for distributed jobs.
    /// </summary>
    internal const string StaticDelay = "00:01:00";

    /// <summary>
    ///     The default maximum execution time for a distributed job.
    /// </summary>
    internal const string StaticMaxExecutionTime = "00:05:00";

    /// <summary>
    ///     Gets or sets a value for the period of checking if there are any runnable distributed jobs.
    /// </summary>
    [DefaultValue(StaticPeriod)]
    public TimeSpan Period { get; set; } = TimeSpan.Parse(StaticPeriod);

    /// <summary>
    ///     Gets or sets a value for the delay of when to start checking for distributed jobs.
    /// </summary>
    [DefaultValue(StaticDelay)]
    public TimeSpan Delay { get; set; } = TimeSpan.Parse(StaticDelay);

    /// <summary>
    ///     Gets or sets the maximum execution time for a distributed job before it is considered timed out.
    ///     When a job exceeds this time, it is considered stale and can be picked up by another server for recovery and restarted.
    /// </summary>
    [DefaultValue(StaticMaxExecutionTime)]
    public TimeSpan MaximumExecutionTime { get; set; } = TimeSpan.Parse(StaticMaxExecutionTime);
}
