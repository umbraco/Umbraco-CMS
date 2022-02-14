using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Core.Telemetry.Models;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Infrastructure.HostedServices
{
    public class ReportSiteTask : RecurringHostedServiceBase
    {
        private readonly ILogger<ReportSiteTask> _logger;
        private readonly ITelemetryService _telemetryService;
        private readonly IJsonSerializer _jsonSerializer;
        private static HttpClient s_httpClient;

        public ReportSiteTask(
            ILogger<ReportSiteTask> logger,
            ITelemetryService telemetryService,
            IJsonSerializer jsonSerializer)
            : base(TimeSpan.FromDays(1), TimeSpan.FromMinutes(0))
        {
            _logger = logger;
            _telemetryService = telemetryService;
            _jsonSerializer = jsonSerializer;

            s_httpClient = new HttpClient()
            {
#if DEBUG
                // Send data to DEBUG telemetry service
                BaseAddress = new Uri("https://telemetry.rainbowsrock.net/")
#else
                // Send data to LIVE telemetry
                BaseAddress = new Uri("https://telemetry.umbraco.com/")
#endif
            };

            s_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        }

        [Obsolete("Use ctor with all params")]
        public ReportSiteTask(
            ILogger<ReportSiteTask> logger,
            ITelemetryService telemetryService)
            : this(logger, telemetryService, StaticServiceProvider.Instance.GetRequiredService<IJsonSerializer>())
        {
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
        public override async Task PerformExecuteAsync(object state)
        {
            if (_telemetryService.TryGetTelemetryReportData(out TelemetryReportData telemetryReportData) is false)
            {
                return;
            }

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, "installs/"))
                {
                    request.Content = new StringContent(_jsonSerializer.Serialize(telemetryReportData), Encoding.UTF8, "application/json");

                    // Make an HTTP POST to telemetry service (fire & forget, do not need to know if it's a 200, 500, etc.)
                    using HttpResponseMessage response = await s_httpClient.SendAsync(request);
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
