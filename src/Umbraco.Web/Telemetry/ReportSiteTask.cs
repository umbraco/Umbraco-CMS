using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Core.Telemetry;
using Umbraco.Web.Scheduling;

namespace Umbraco.Web.Telemetry
{
    public class ReportSiteTask : RecurringTaskBase
    {
        private readonly IProfilingLogger _logger;
        private static HttpClient _httpClient;
        private readonly ITelemetryService _telemetryService;

        public ReportSiteTask(
            IBackgroundTaskRunner<RecurringTaskBase> runner,
            int delayBeforeWeStart,
            int howOftenWeRepeat,
            IProfilingLogger logger,
            ITelemetryService telemetryService)
            : base(runner, delayBeforeWeStart, howOftenWeRepeat)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            _telemetryService = telemetryService;
        }

        /// <summary>
        /// Runs the background task to send the anonymous ID
        /// to telemetry service
        /// </summary>
        /// <returns>A value indicating whether to repeat the task.</returns>
        public override async Task<bool> PerformRunAsync(CancellationToken token)
        {
            // Parse as a GUID & verify its a GUID and not some random string
            // In case of users may have messed or decided to empty the file contents or put in something random
            if (_telemetryService.TryGetTelemetryReportData(out var telemetryReportData) is false)
            {
                // Some users may have decided to mess with the XML attribute and put in something else
                // Stop repeating this task (no need to keep checking)
                // The only time it will recheck when the site is recycled
                return false;
            }

            try
            {
                // Send data to LIVE telemetry
                _httpClient.BaseAddress = new Uri("https://telemetry.umbraco.com/");

#if DEBUG
                // Send data to DEBUG telemetry service
                _httpClient.BaseAddress = new Uri("https://telemetry.rainbowsrock.net/");
#endif

                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                using (var request = new HttpRequestMessage(HttpMethod.Post, "installs/"))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(telemetryReportData), Encoding.UTF8, "application/json"); //CONTENT-TYPE header

                    // Set a low timeout - no need to use a larger default timeout for this POST request
                    _httpClient.Timeout = new TimeSpan(0, 0, 1);

                    // Make a HTTP Post to telemetry service
                    // https://telemetry.umbraco.com/installs/
                    // Fire & Forget, do not need to know if its a 200, 500 etc
                    var result = await _httpClient.SendAsync(request, token);
                }
            }
            catch
            {
                // Silently swallow
                // The user does not need the logs being polluted if our service has fallen over or is down etc
                // Hence only logging this at a more verbose level (which users should not be using in production)
                _logger.Debug<ReportSiteTask>("There was a problem sending a request to the Umbraco telemetry service");
            }

            // Keep recurring this task & pinging the telemetry service
            return true;
        }

        public override bool IsAsync => true;
    }
}
