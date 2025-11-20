// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Typed configuration options for logging settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigLogging)]
public class LoggingSettings
{
    internal const string StaticMaxLogAge = "1.00:00:00"; // TimeSpan.FromHours(24);
    internal const string StaticDirectory = Constants.SystemDirectories.LogFiles;
    internal const string StaticFileNameFormat = LoggingConfiguration.DefaultLogFileNameFormat;
    internal const string StaticFileNameFormatArguments = "MachineName";

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

    /// <summary>
    /// Gets or sets the file name format to use for log files.
    /// </summary>
    /// <value>
    /// The file name format.
    /// </value>
    [DefaultValue(StaticFileNameFormat)]
    public string FileNameFormat { get; set; } = StaticFileNameFormat;

    /// <summary>
    /// Gets or sets the file name format arguments to use for log files.
    /// </summary>
    /// <value>
    /// The file name format arguments as a comma delimited string of accepted values.
    /// </value>
    /// <remarks>
    /// Accepted values for format arguments are: MachineName, EnvironmentName.
    /// </remarks>
    [DefaultValue(StaticFileNameFormatArguments)]
    public string FileNameFormatArguments { get; set; } = StaticFileNameFormatArguments;
}
