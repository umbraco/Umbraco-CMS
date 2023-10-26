using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Umbraco.Cms.Infrastructure.Logging.Serilog;

public class UmbracoFileConfiguration
{
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

            RestrictedToMinimumLevel = args.GetValue(nameof(RestrictedToMinimumLevel), RestrictedToMinimumLevel);
            FileSizeLimitBytes = args.GetValue(nameof(FileSizeLimitBytes), FileSizeLimitBytes);
            RollingInterval = args.GetValue(nameof(RollingInterval), RollingInterval);
            FlushToDiskInterval = args.GetValue(nameof(FlushToDiskInterval), FlushToDiskInterval);
            RollOnFileSizeLimit = args.GetValue(nameof(RollOnFileSizeLimit), RollOnFileSizeLimit);
            RetainedFileCountLimit = args.GetValue(nameof(RetainedFileCountLimit), RetainedFileCountLimit);
        }
    }

    public LogEventLevel RestrictedToMinimumLevel { get; set; } = LogEventLevel.Verbose;

    public long FileSizeLimitBytes { get; set; } = 1073741824;

    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;

    public TimeSpan? FlushToDiskInterval { get; set; }

    public bool RollOnFileSizeLimit { get; set; }

    public int RetainedFileCountLimit { get; set; } = 31;

    public string GetPath(string logDirectory) =>
        Path.Combine(logDirectory, $"UmbracoTraceLog.{Environment.MachineName}..json");
}
