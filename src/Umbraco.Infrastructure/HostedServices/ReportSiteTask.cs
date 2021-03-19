using System;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HostedServices
{
    public class ReportSiteTask : RecurringHostedServiceBase
    {
        private readonly ILogger<ReportSiteTask> _logger;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IOptions<GlobalSettings> _globalSettings;
        private static HttpClient s_httpClient;

        public ReportSiteTask(
            ILogger<ReportSiteTask> logger,
            IUmbracoVersion umbracoVersion,
            IOptions<GlobalSettings> globalSettings)
            : base(TimeSpan.FromDays(1), TimeSpan.FromMinutes(1))
        {
            _logger = logger;
            _umbracoVersion = umbracoVersion;
            _globalSettings = globalSettings;
            s_httpClient = new HttpClient();
        }

        /// <summary>
        /// Runs the background task to send the anonymous ID
        /// to telemetry service
        /// </summary>
        public override async Task PerformExecuteAsync(object state)
        {
            // Try & get a value stored in umbracoSettings.config on the backoffice XML element ID attribute
            var backofficeIdentifierRaw = _globalSettings.Value.Id;

            // Parse as a GUID & verify its a GUID and not some random string
            // In case of users may have messed or decided to empty the file contents or put in something random
            if (Guid.TryParse(backofficeIdentifierRaw, out var telemetrySiteIdentifier) == false)
            {
                // Some users may have decided to mess with the XML attribute and put in something else
                _logger.LogWarning("No telemetry marker found");

                return;
            }

            try
            {
                // Send data to LIVE telemetry
                s_httpClient.BaseAddress = new Uri("https://telemetry.umbraco.com/");

#if DEBUG
                // Send data to DEBUG telemetry service
                s_httpClient.BaseAddress = new Uri("https://telemetry.rainbowsrock.net/");
#endif

                s_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                using (var request = new HttpRequestMessage(HttpMethod.Post, "installs/"))
                {
                    var postData = new TelemetryReportData { Id = telemetrySiteIdentifier, Version = _umbracoVersion.SemanticVersion.ToSemanticString() };
                    request.Content = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json"); //CONTENT-TYPE header

                    // Set a low timeout - no need to use a larger default timeout for this POST request
                    s_httpClient.Timeout = new TimeSpan(0, 0, 1);

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
        [DataContract]
        private class TelemetryReportData
        {
            [DataMember(Name = "id")]
            public Guid Id { get; set; }

            [DataMember(Name = "version")]
            public string Version { get; set; }
        }


    }
}
