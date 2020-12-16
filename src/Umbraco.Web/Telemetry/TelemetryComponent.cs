using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Web.Scheduling;

namespace Umbraco.Web.Telemetry
{
    public class TelemetryComponent : IComponent
    {
        private IProfilingLogger _logger;
        private BackgroundTaskRunner<IBackgroundTask> _telemetryReporterRunner;

        public TelemetryComponent(IProfilingLogger logger)
        {
            _logger = logger;
        }

        public void Initialize()
        {
            // backgrounds runners are web aware, if the app domain dies, these tasks will wind down correctly
            _telemetryReporterRunner = new BackgroundTaskRunner<IBackgroundTask>("TelemetryReporter", _logger);

            int delayBeforeWeStart = 60 * 1000; // 60 * 1000ms = 1min (60,000)
            int howOftenWeRepeat = 60 * 1000 * 60 * 24; // 60 * 1000 * 60 * 24 = 24hrs (86400000)

            // As soon as we add our task to the runner it will start to run (after its delay period)
            var task = new ReportSiteTask(_telemetryReporterRunner, delayBeforeWeStart, howOftenWeRepeat, _logger);
            _telemetryReporterRunner.TryAdd(task);
        }

        public void Terminate()
        {
        }
    }
}
