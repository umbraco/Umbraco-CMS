// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for hosting settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigHosting)]
public class HostingSettings
{
    internal const string StaticLocalTempStorageLocation = "Default";
    internal const bool StaticDebug = false;

    /// <summary>
    ///     Gets or sets a value for the application virtual path.
    /// </summary>
    public string? ApplicationVirtualPath { get; set; }

    /// <summary>
    ///     Gets or sets a value for the location of temporary files.
    /// </summary>
    [DefaultValue(StaticLocalTempStorageLocation)]
    public LocalTempStorage LocalTempStorageLocation { get; set; } =
        Enum<LocalTempStorage>.Parse(StaticLocalTempStorageLocation);

    /// <summary>
    ///     Gets or sets a value indicating whether umbraco is running in [debug mode].
    /// </summary>
    /// <value><c>true</c> if [debug mode]; otherwise, <c>false</c>.</value>
    [DefaultValue(StaticDebug)]
    public bool Debug { get; set; } = StaticDebug;

    /// <summary>
    ///     Gets or sets a value specifying the name of the site.
    /// </summary>
    public string? SiteName { get; set; }
}
