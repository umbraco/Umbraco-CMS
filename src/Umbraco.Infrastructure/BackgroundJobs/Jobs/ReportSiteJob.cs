using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

public class ReportSiteJob : IRecurringBackgroundJob
{
    public TimeSpan Period => TimeSpan.FromDays(1);

    public TimeSpan Delay => TimeSpan.FromMinutes(5);

    public ServerRole[] ServerRoles => Enum.GetValues<ServerRole>();

    // No-op event as the period never changes on this job
    public event EventHandler PeriodChanged
    {
        add { }
        remove { }
    }

    private readonly ILogger<ReportSiteJob> _logger;
    private readonly ITelemetryService _telemetryService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IHttpClientFactory _httpClientFactory;

    [Obsolete("Use the constructor with IHttpClientFactory instead.")]
    public ReportSiteJob(
        ILogger<ReportSiteJob> logger,
        ITelemetryService telemetryService,
        IJsonSerializer jsonSerializer)
        : this(
            logger,
            telemetryService,
            jsonSerializer,
            StaticServiceProvider.Instance.GetRequiredService<IHttpClientFactory>())
    { }

    public ReportSiteJob(
        ILogger<ReportSiteJob> logger,
        ITelemetryService telemetryService,
        IJsonSerializer jsonSerializer,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _telemetryService = telemetryService;
        _jsonSerializer = jsonSerializer;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Runs the background task to send the anonymous ID
    /// to telemetry service
    /// </summary>
    public async Task RunJobAsync()
    {
        TelemetryReportData? telemetryReportData = await _telemetryService.GetTelemetryReportDataAsync().ConfigureAwait(false);
        if (telemetryReportData is null)
        {
            _logger.LogWarning("No telemetry marker found");

            return;
        }

        try
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            if (httpClient.BaseAddress is null)
            {
                // Send data to LIVE telemetry
                httpClient.BaseAddress = new Uri("https://telemetry.umbraco.com/");

#if DEBUG
                // Send data to DEBUG telemetry service
                httpClient.BaseAddress = new Uri("https://telemetry.rainbowsrock.net/");
#endif
            }

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "installs/");
            request.Content = new StringContent(_jsonSerializer.Serialize(telemetryReportData), Encoding.UTF8, "application/json");

            // Make a HTTP Post to telemetry service
            // https://telemetry.umbraco.com/installs/
            // Fire & Forget, do not need to know if its a 200, 500 etc
            using (await httpClient.SendAsync(request))
            { }
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
