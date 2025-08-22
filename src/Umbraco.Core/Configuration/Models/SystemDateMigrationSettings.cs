// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Typed configuration options used for migration of system dates to UTC from a Umbraco 16 or lower solution.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigSystemDateMigration)]
public class SystemDateMigrationSettings
{
    private const bool StaticEnabled = true;

    /// <summary>
    /// Gets or sets a value indicating whether the migration is enabled.
    /// </summary>
    [DefaultValue(StaticEnabled)]
    public bool Enabled { get; set; } = StaticEnabled;

    /// <summary>
    /// Gets or sets the local server timezone standard name.
    /// If not provided, the local server time zone is detected.
    /// </summary>
    [DefaultValue(null)]
    public string? LocalServerTimeZone { get; set; }
}
