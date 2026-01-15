// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for long-running operations settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigLongRunningOperations)]
public class LongRunningOperationsSettings
{
    private const string StaticExpirationTime = "00:05:00";
    private const string StaticTimeBetweenStatusChecks = "00:00:10";

    /// <summary>
    /// Gets or sets the cleanup settings for long-running operations.
    /// </summary>
    public LongRunningOperationsCleanupSettings Cleanup { get; set; } = new();

    /// <summary>
    /// Gets or sets the time after which a long-running operation is considered expired/stale, if not updated.
    /// </summary>
    [DefaultValue(StaticExpirationTime)]
    public TimeSpan ExpirationTime { get; set; } = TimeSpan.Parse(StaticExpirationTime);

    /// <summary>
    /// Gets or sets the time between status checks for long-running operations.
    /// </summary>
    [DefaultValue(StaticTimeBetweenStatusChecks)]
    public TimeSpan TimeBetweenStatusChecks { get; set; } = TimeSpan.Parse(StaticTimeBetweenStatusChecks);
}
