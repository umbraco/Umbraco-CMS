// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Typed configuration options for logging settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigLogging)]
public class LoggingSettings
{
    internal const string StaticMaxLogAge = "1.00:00:00"; // TimeSpan.FromHours(24);
    internal const string StaticDirectory = Constants.SystemDirectories.LogFiles;

    /// <summary>
    /// Gets or sets a value for the maximum age of a log file.
    /// </summary>
    /// <value>
    /// The maximum log age.
    /// </value>
    [DefaultValue(StaticMaxLogAge)]
    public TimeSpan MaxLogAge { get; set; } = TimeSpan.Parse(StaticMaxLogAge);

    /// <summary>
    /// Gets or sets the folder to use for log files.
    /// </summary>
    /// <value>
    /// The directory.
    /// </value>
    [DefaultValue(StaticDirectory)]
    public string Directory { get; set; } = StaticDirectory;
}
