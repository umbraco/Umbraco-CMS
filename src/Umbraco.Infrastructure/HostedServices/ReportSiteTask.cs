using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Core.Telemetry.Models;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Infrastructure.HostedServices
{
    public class ReportSiteTask : RecurringHostedServiceBase
    {
        private readonly ILogger<ReportSiteTask> _logger;
        private readonly ITelemetryService _telemetryService;
        private static HttpClient s_httpClient = new();

        public ReportSiteTask(
            ILogger<ReportSiteTask> logger,
            ITelemetryService telemetryService)
            : base(TimeSpan.FromDays(1), TimeSpan.FromMinutes(1))
        {
            _logger = logger;
            _telemetryService = telemetryService;
            s_httpClient = new HttpClient();
        }

        [Obsolete("Use the constructor that takes ITelemetryService instead, scheduled for removal in V11")]
        public ReportSiteTask(
            ILogger<ReportSiteTask> logger,
            IUmbracoVersion umbracoVersion,
            IOptions<GlobalSettings> globalSettings)
            : this(logger, StaticServiceProvider.Instance.GetRequiredService<ITelemetryService>())
        {
        }

        /// <summary>
        /// Runs the background task to send the anonymous ID
        /// to telemetry service
        /// </summary>
        public override async Task PerformExecuteAsync(object? state)
        {
            if (_telemetryService.TryGetTelemetryReportData(out TelemetryReportData? telemetryReportData) is false)
            {
                _logger.LogWarning("No telemetry marker found");

                return;
            }

            try
            {
                if (s_httpClient.BaseAddress is null)
                {
                    // Send data to LIVE telemetry
                    s_httpClient.BaseAddress = new Uri("https://telemetry.umbraco.com/");

#if DEBUG
                    // Send data to DEBUG telemetry service
                    s_httpClient.BaseAddress = new Uri("https://telemetry.rainbowsrock.net/");
#endif
                }


                s_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                using (var request = new HttpRequestMessage(HttpMethod.Post, "installs/"))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(telemetryReportData), Encoding.UTF8, "application/json"); //CONTENT-TYPE header

                    // Make a HTTP Post to telemetry service
                    // https://telemetry.umbraco.com/installs/
                    // Fire & Forget, do not need to know if its a 200, 500 etc
                    using (HttpResponseMessage response = await s_httpClient.SendAsync(request))
                    {
                    }
                }
            }
            catch
            {
                // Silently swallow
                // The user does not need the logs being polluted if our service has fallen over or is down etc
                // Hence only logging this at a more verbose level (which users should not be using in production)
                _logger.LogDebug("There was a problem sending a request to the Umbraco telemetry service");
            }
        }
    }
}
