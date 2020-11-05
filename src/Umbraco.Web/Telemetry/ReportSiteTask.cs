using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Scheduling;

namespace Umbraco.Web.Telemetry
{
    public class ReportSiteTask : RecurringTaskBase
    {
        private IRuntimeState _runtime;
        private IProfilingLogger _logger;
        private static HttpClient _httpClient;

        public ReportSiteTask(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayBeforeWeStart, int howOftenWeRepeat, IRuntimeState runtime, IProfilingLogger logger)
            : base(runner, delayBeforeWeStart, howOftenWeRepeat)
        {
            _runtime = runtime;
            _logger = logger;

            if (_httpClient == null)
                _httpClient = new HttpClient();
        }

        /// <summary>
        /// Runs the background task to send the anynomous ID
        /// to telemetry service
        /// </summary>
        /// <returns>A value indicating whether to repeat the task.</returns>
        public override async Task<bool> PerformRunAsync(CancellationToken token)
        {
            // Try & find file at '/umbraco/telemetrics-id.umb'
            var telemetricsFilePath = IOHelper.MapPath(SystemFiles.TelemetricsIdentifier);

            if (File.Exists(telemetricsFilePath) == false)
            {
                // Some users may have decided to not be tracked by deleting/removing the marker file
                _logger.Warn<ReportSiteTask>("No telemetry marker file found at '{filePath}' and will not report site to telemetry service", telemetricsFilePath);

                // Stop repeating this task (no need to keep checking)
                // The only time it will recheck when the site is recycled
                return false;
            }


            var telemetricsFileContents = string.Empty;
            try
            {
                // Open file & read its contents
                // It may throw due to file permissions or file locking
                telemetricsFileContents = File.ReadAllText(telemetricsFilePath);
            }
            catch (Exception ex)
            {
                // Silently swallow ex - but lets log it (ReadAllText throws a ton of different types of ex)
                // Hence the use of general exception type
                _logger.Error<ReportSiteTask>(ex, "Error in reading file contents of telemetry marker file found at '{filePath}'", telemetricsFilePath);
            }


            // Parse as a GUID & verify its a GUID and not some random string
            // In case of users may have messed or decided to empty the file contents or put in something random
            if (Guid.TryParse(telemetricsFileContents, out var telemetrySiteIdentifier) == false)
            {
                // Some users may have decided to mess with file contents
                _logger.Warn<ReportSiteTask>("The telemetry marker file found at '{filePath}' with '{telemetrySiteId}' is not a valid identifier for the telemetry service", telemetricsFilePath, telemetrySiteIdentifier);

                // Stop repeating this task (no need to keep checking)
                // The only time it will recheck when the site is recycled
                return false;
            }


            try
            {
                // Make a HTTP Post to telemetry service
                // https://telemetry.umbraco.com/installs
                // Fire & Forget, do not need to know if its a 200, 500 etc
                var postData = new TelemetryReportData { Id = telemetrySiteIdentifier, Version = UmbracoVersion.SemanticVersion.ToSemanticString() };
                var result = await _httpClient.PostAsync("https://telemetry.rainbowsrock.net/installs", postData, new JsonMediaTypeFormatter());
            }
            catch (Exception ex)
            {
                // Silently swallow
                // The user does need logs being polluted if our service has fallen over or is down etc
            }

            // Keep recurring this task & pinging the telemetry service
            return true;
        }

        public override bool IsAsync => true;


        private class TelemetryReportData
        {
            public Guid Id { get; set; }

            public string Version { get; set; }
        }
    }
}

