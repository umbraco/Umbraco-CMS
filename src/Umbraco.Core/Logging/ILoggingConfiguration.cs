namespace Umbraco.Cms.Core.Logging;

/// <summary>
///     Defines the configuration settings for the logging system.
/// </summary>
public interface ILoggingConfiguration
{
    /// <summary>
    ///     Gets the physical path where logs are stored.
    /// </summary>
    string LogDirectory { get; }

    /// <summary>
    ///     Gets the file name format for the log files.
    /// </summary>
    string LogFileNameFormat { get; }

    /// <summary>
    ///     Gets the file name format arguments for the log files.
    /// </summary>
    string[] GetLogFileNameFormatArguments();
}
