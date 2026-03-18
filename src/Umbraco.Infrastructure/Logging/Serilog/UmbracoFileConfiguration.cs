using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Infrastructure.Logging.Serilog;

/// <summary>
/// Provides configuration settings for file-based logging in Umbraco using Serilog.
/// </summary>
public class UmbracoFileConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoFileConfiguration"/> class using the specified configuration.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance used to configure the file settings.</param>
    public UmbracoFileConfiguration(IConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        IConfigurationSection? appSettings = configuration.GetSection("Serilog:WriteTo");
        IConfigurationSection? umbracoFileAppSettings =
            appSettings.GetChildren().LastOrDefault(x => x.GetValue<string>("Name") == "UmbracoFile");

        if (umbracoFileAppSettings is not null)
        {
            IConfigurationSection? args = umbracoFileAppSettings.GetSection("Args");

            Enabled = args.GetValue(nameof(Enabled), Enabled);
            RestrictedToMinimumLevel = args.GetValue(nameof(RestrictedToMinimumLevel), RestrictedToMinimumLevel);
            FileSizeLimitBytes = args.GetValue(nameof(FileSizeLimitBytes), FileSizeLimitBytes);
            RollingInterval = args.GetValue(nameof(RollingInterval), RollingInterval);
            FlushToDiskInterval = args.GetValue(nameof(FlushToDiskInterval), FlushToDiskInterval);
            RollOnFileSizeLimit = args.GetValue(nameof(RollOnFileSizeLimit), RollOnFileSizeLimit);
            RetainedFileCountLimit = args.GetValue(nameof(RetainedFileCountLimit), RetainedFileCountLimit);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether file logging for Umbraco is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum log event level allowed for logging.
    /// </summary>
    public LogEventLevel RestrictedToMinimumLevel { get; set; } = LogEventLevel.Verbose;

    /// <summary>
    /// Gets or sets the maximum size, in bytes, that a log file can reach before it is rolled over.
    /// </summary>
    public long FileSizeLimitBytes { get; set; } = 1073741824;

    /// <summary>
    /// Gets or sets the interval that determines how frequently a new log file is created (for example, daily or hourly).
    /// </summary>
    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;

    /// <summary>
    /// Gets or sets the interval at which the log file buffer is flushed to disk.
    /// A shorter interval may reduce data loss in case of a crash, but can impact performance.
    /// </summary>
    public TimeSpan? FlushToDiskInterval { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the log file should be rolled over when it reaches the configured file size limit.
    /// </summary>
    public bool RollOnFileSizeLimit { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of log files to retain.
    /// </summary>
    public int RetainedFileCountLimit { get; set; } = 31;

    /// <summary>
    /// Gets the full path for the log file based on the specified log directory.
    /// </summary>
    /// <param name="logDirectory">The directory where log files are stored.</param>
    /// <returns>The full path to the log file.</returns>
    public string GetPath(string logDirectory) =>
        GetPath(logDirectory, LoggingConfiguration.DefaultLogFileNameFormat, Environment.MachineName);

    /// <summary>
    /// Constructs the full path for the log file based on the specified directory, file name format, and arguments.
    /// </summary>
    /// <param name="logDirectory">The directory where the log file will be stored.</param>
    /// <param name="fileNameFormat">The format string used to generate the log file name.</param>
    /// <param name="fileNameArgs">Optional arguments to format the file name.</param>
    /// <returns>The full path of the log file as a string.</returns>
    public string GetPath(string logDirectory, string fileNameFormat, params string[] fileNameArgs) =>
        Path.Combine(logDirectory, string.Format(fileNameFormat, fileNameArgs));
}
