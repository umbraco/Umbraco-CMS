// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Typed configuration options for runtime settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigRuntime)]
public class RuntimeSettings
{
    private const string StaticTemporaryFileLifeTime = "1.00:00:00"; // TimeSpan.FromDays(1);

    /// <summary>
    /// Gets or sets the runtime mode.
    /// </summary>
    [DefaultValue(RuntimeMode.BackofficeDevelopment)]
    public RuntimeMode Mode { get; set; } = RuntimeMode.BackofficeDevelopment;

    /// <summary>
    /// Gets or sets which migrations the runtime performs at boot when the database is not at the final state of a migration plan.
    /// </summary>
    /// <remarks>
    /// Read when the runtime level is determined at boot, so a change takes effect on the next restart.
    /// </remarks>
    [DefaultValue(MigrationBehavior.Upgrade)]
    public MigrationBehavior MigrationBehavior { get; set; } = MigrationBehavior.Upgrade;

    /// <summary>
    /// Gets or sets the maximum length of the request in kilobytes.
    /// </summary>
    public long? MaxRequestLength { get; set; }

    /// <summary>
    /// Gets or sets the timespan temporary files are kept, before they are removed by a background task.
    /// </summary>
    [DefaultValue(StaticTemporaryFileLifeTime)]
    public TimeSpan TemporaryFileLifeTime { get; set; } = TimeSpan.Parse(StaticTemporaryFileLifeTime);
}
