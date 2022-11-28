// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.Extensions;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for logging settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigLogging)]
public class LoggingSettings
{
    internal const string StaticMaxLogAge = "1.00:00:00"; // TimeSpan.FromHours(24);
    internal const string StaticLoggingDirectory = Constants.SystemDirectories.LogFiles;

    /// <summary>
    ///     Gets or sets a value for the maximum age of a log file.
    /// </summary>
    [DefaultValue(StaticMaxLogAge)]
    public TimeSpan MaxLogAge { get; set; } = TimeSpan.Parse(StaticMaxLogAge);

    /// <summary>
    ///     Gets or sets the folder to use for log files
    /// </summary>
    [DefaultValue(StaticLoggingDirectory)]
    public string LoggingDirectory { get; set; } = StaticLoggingDirectory;

    public string GetAbsoluteLoggingPath(IHostEnvironment hostEnvironment)
    {
        var dir = LoggingDirectory;
        if (dir.StartsWith("~/"))
        {
            hostEnvironment.MapPathContentRoot(dir);
        }

        return dir;
    }
}
