// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for long-running operations cleanup settings.
/// </summary>
public class LongRunningOperationsCleanupSettings
{
    private const string StaticPeriod = "00:02:00";
    private const string StaticMaxAge = "01:00:00";

    /// <summary>
    /// Gets or sets a value for the period in which long-running operations are cleaned up.
    /// </summary>
    [DefaultValue(StaticPeriod)]
    public TimeSpan Period { get; set; } = TimeSpan.Parse(StaticPeriod);

    /// <summary>
    /// Gets or sets the maximum time a long-running operation entry can exist, without being updated, before it is considered for cleanup.
    /// </summary>
    [DefaultValue(StaticMaxAge)]
    public TimeSpan MaxEntryAge { get; set; } = TimeSpan.Parse(StaticMaxAge);
}
