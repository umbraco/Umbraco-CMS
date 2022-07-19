using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Core.Telemetry.Models;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Infrastructure.HostedServices;

public class ReportSiteTask : RecurringHostedServiceBase
{
    private static HttpClient _httpClient = new();
    private readonly ILogger<ReportSiteTask> _logger;
    private readonly ITelemetryService _telemetryService;
    private readonly IRuntimeState _runtimeState;

    public ReportSiteTask(
        ILogger<ReportSiteTask> logger,
        ITelemetryService telemetryService,
        IRuntimeState runtimeState)
        : base(logger, TimeSpan.FromDays(1), TimeSpan.FromMinutes(5))
    {
        _logger = logger;
        _telemetryService = telemetryService;
        _runtimeState = runtimeState;
        _httpClient = new HttpClient();
    }

    [Obsolete("Use the constructor that takes IRuntimeState, scheduled for removal in V12")]
    public ReportSiteTask(
        ILogger<ReportSiteTask> logger,
        ITelemetryService telemetryService)
        : this(logger, telemetryService, StaticServiceProvider.Instance.GetRequiredService<IRuntimeState>())
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
    ///     Runs the background task to send the anonymous ID
    ///     to telemetry service
    /// </summary>
    public override async Task PerformExecuteAsync(object? state)
    {
        if (_runtimeState.Level is not RuntimeLevel.Run)
        {
            // We probably haven't installed yet, so we can't get telemetry.
            return;
        }

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
