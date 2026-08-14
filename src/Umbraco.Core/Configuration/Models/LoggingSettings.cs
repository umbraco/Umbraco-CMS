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
    /// <summary>
    ///     The default maximum age for log files.
    /// </summary>
    internal const string StaticMaxLogAge = "1.00:00:00"; // TimeSpan.FromHours(24);

    /// <summary>
    ///     The default directory for log files.
    /// </summary>
    internal const string StaticDirectory = "~/umbraco/Logs";

    /// <summary>
    ///     The default file name format for log files.
    /// </summary>
    internal const string StaticFileNameFormat = LoggingConfiguration.DefaultLogFileNameFormat;

    /// <summary>
    ///     The default file name format arguments for log files.
    /// </summary>
    internal const string StaticFileNameFormatArguments = "MachineName";

    /// <summary>
    ///     The default mode for enriching log events with a session identifier.
    /// </summary>
    internal const SessionIdLoggingMode StaticSessionIdLogging = SessionIdLoggingMode.SessionId;

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

    /// <summary>
    /// Gets or sets a value determining how log events are enriched with a session identifier.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="SessionIdLoggingMode.SessionId" /> for backward compatibility. Set to
    /// <see cref="SessionIdLoggingMode.CookieHash" /> or <see cref="SessionIdLoggingMode.None" /> to avoid the
    /// blocking session-store load that resolving the actual session id incurs per request when the session is
    /// backed by an <c>IDistributedCache</c>.
    /// </remarks>
    [DefaultValue(StaticSessionIdLogging)]
    public SessionIdLoggingMode SessionIdLogging { get; set; } = StaticSessionIdLogging;
}
