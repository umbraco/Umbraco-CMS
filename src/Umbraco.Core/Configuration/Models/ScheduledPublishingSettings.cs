using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Settings for scheduled publishing.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigScheduledPublishing)]
public class ScheduledPublishingSettings
{
    private const string StaticPeriod = "00:01:00";
    private const bool StaticAlignToClock = false; // TODO (V19): Switch this to true.

    /// <summary>
    /// Gets or sets a value for how often scheduled publishing runs.
    /// </summary>
    [DefaultValue(StaticPeriod)]
    public TimeSpan Period { get; set; } = TimeSpan.Parse(StaticPeriod);

    /// <summary>
    /// Gets or sets a value indicating whether scheduled publishing runs are aligned to clock boundaries
    /// derived from <see cref="Period" /> (for example, on the minute, or every N seconds), rather than drifting
    /// based on when the previous run completed.
    /// </summary>
    /// <remarks>
    /// When enabled, <see cref="Period" /> must be a whole number of seconds that divides evenly into one hour
    /// (for example 10, 12, 15, 20, 30 or 60 seconds) so that boundaries land on consistent clock times.
    /// Boundaries are anchored to <strong>UTC</strong>, not the server's local time zone; for sub-minute and
    /// whole-minute periods this is indistinguishable from local time at the second level.
    /// </remarks>
    [DefaultValue(StaticAlignToClock)]
    public bool AlignToClock { get; set; } = StaticAlignToClock;
}
