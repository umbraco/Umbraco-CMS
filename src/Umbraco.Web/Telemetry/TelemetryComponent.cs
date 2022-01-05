using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Telemetry;
using Umbraco.Web.Scheduling;

namespace Umbraco.Web.Telemetry
{
    public class TelemetryComponent : IComponent
    {
        private readonly IProfilingLogger _logger;
        private readonly ITelemetryService _telemetryService;
        private BackgroundTaskRunner<IBackgroundTask> _telemetryReporterRunner;

        public TelemetryComponent(IProfilingLogger logger, IUmbracoSettingsSection settings, ITelemetryService telemetryService)
        {
            _logger = logger;
            _telemetryService = telemetryService;
        }

        public void Initialize()
        {
            // backgrounds runners are web aware, if the app domain dies, these tasks will wind down correctly
            _telemetryReporterRunner = new BackgroundTaskRunner<IBackgroundTask>("TelemetryReporter", _logger);

            const int delayBeforeWeStart = 60 * 1000; // 60 * 1000ms = 1min (60,000)
            const int howOftenWeRepeat = 60 * 1000 * 60 * 24; // 60 * 1000 * 60 * 24 = 24hrs (86400000)

            // As soon as we add our task to the runner it will start to run (after its delay period)
            var task = new ReportSiteTask(_telemetryReporterRunner, delayBeforeWeStart, howOftenWeRepeat, _logger, _telemetryService);
            _telemetryReporterRunner.TryAdd(task);
        }

        public void Terminate()
        {
        }
    }
}
