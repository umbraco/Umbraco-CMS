using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

public class ReportSiteJob : IRecurringBackgroundJob
{

    public TimeSpan Period { get => TimeSpan.FromDays(1); }
    public TimeSpan Delay { get => TimeSpan.FromMinutes(5); }
    public ServerRole[] ServerRoles { get => Enum.GetValues<ServerRole>(); }

    // No-op event as the period never changes on this job
    public event EventHandler PeriodChanged { add { } remove { } }


    private static HttpClient _httpClient = new();
    private readonly ILogger<ReportSiteJob> _logger;
    private readonly ITelemetryService _telemetryService;
    

    public ReportSiteJob(
        ILogger<ReportSiteJob> logger,
        ITelemetryService telemetryService)
    {
        _logger = logger;
        _telemetryService = telemetryService;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Runs the background task to send the anonymous ID
    /// to telemetry service
    /// </summary>
    public  async Task RunJobAsync()
    {

        if (_telemetryService.TryGetTelemetryReportData(out TelemetryReportData? telemetryReportData) is false)
        {
            _logger.LogWarning("No telemetry marker found");

            return;
        }

        try
        {
            if (_httpClient.BaseAddress is null)
            {
                // Send data to LIVE telemetry
                _httpClient.BaseAddress = new Uri("https://telemetry.umbraco.com/");

#if DEBUG
                // Send data to DEBUG telemetry service
                _httpClient.BaseAddress = new Uri("https://telemetry.rainbowsrock.net/");
#endif
            }

            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

            using (var request = new HttpRequestMessage(HttpMethod.Post, "installs/"))
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(telemetryReportData), Encoding.UTF8,
                    "application/json");

                // Make a HTTP Post to telemetry service
                // https://telemetry.umbraco.com/installs/
                // Fire & Forget, do not need to know if its a 200, 500 etc
                using (await _httpClient.SendAsync(request))
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
