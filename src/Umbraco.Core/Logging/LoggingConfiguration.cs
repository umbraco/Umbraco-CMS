namespace Umbraco.Cms.Core.Logging;

/// <summary>
/// Implements <see cref="ILoggingConfiguration"/> to provide configuration for logging to files.
/// </summary>
public class LoggingConfiguration : ILoggingConfiguration
{
    /// <summary>
    /// The default log file name format.
    /// </summary>
    public const string DefaultLogFileNameFormat = "UmbracoTraceLog.{0}..json";

    /// <summary>
    /// The default log file name format arguments.
    /// </summary>
    public const string DefaultLogFileNameFormatArguments = MachineNameFileFormatArgument;

    /// <summary>
    /// The collection of supported file name format arguments.
    /// </summary>
    public static readonly string[] SupportedFileNameFormatArguments =
    {
        MachineNameFileFormatArgument,
        EnvironmentNameFileFormatArgument,
    };

    private readonly string _logFileNameFormatArguments;

    private const string MachineNameFileFormatArgument = "MachineName";
    private const string EnvironmentNameFileFormatArgument = "EnvironmentName";

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingConfiguration"/> class with the default log file name format and arguments.
    /// </summary>
    /// <param name="logDirectory">The log file directory.</param>
    public LoggingConfiguration(string logDirectory)
        : this(logDirectory, DefaultLogFileNameFormat, DefaultLogFileNameFormatArguments)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingConfiguration"/> class.
    /// </summary>
    /// <param name="logDirectory">The log file directory.</param>
    /// <param name="logFileNameFormat">The log file name format.</param>
    /// <param name="logFileNameFormatArguments">The log file name format arguments as a comma delimited string.</param>
    public LoggingConfiguration(string logDirectory, string logFileNameFormat, string logFileNameFormatArguments)
    {
        LogDirectory = logDirectory;
        LogFileNameFormat = logFileNameFormat;
        _logFileNameFormatArguments = logFileNameFormatArguments;
    }

    /// <inheritdoc/>
    public string LogDirectory { get; }

    /// <inheritdoc/>
    public string LogFileNameFormat { get; }

    /// <inheritdoc/>
    public string[] GetLogFileNameFormatArguments() => _logFileNameFormatArguments.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(x => x.Trim())
        .Select(GetValue)
        .ToArray();

    private static string GetValue(string arg) =>
        arg switch
        {
            MachineNameFileFormatArgument => Environment.MachineName,
            EnvironmentNameFileFormatArgument => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            _ => string.Empty,
        };
}
