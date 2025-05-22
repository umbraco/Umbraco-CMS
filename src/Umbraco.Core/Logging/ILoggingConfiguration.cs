namespace Umbraco.Cms.Core.Logging;

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
