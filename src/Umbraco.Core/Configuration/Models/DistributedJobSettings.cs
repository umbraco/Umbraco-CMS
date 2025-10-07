using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Settings for distributed jobs.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigDistributedJobs)]
public class DistributedJobSettings
{
    internal const string StaticPeriod = "00:00:10";
    internal const string StaticDelay = "00:01:00";

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
}
