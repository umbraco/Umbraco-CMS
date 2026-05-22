using System.Text;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
/// Represents a background job that collects and reports information about the current Umbraco site, typically for analytics, diagnostics, or telemetry purposes.
/// </summary>
public class ReportSiteJob : RecurringBackgroundJobBase
{
    /// <summary>
    /// Gets the period at which the report site job runs.
    /// </summary>
    public override TimeSpan Period => TimeSpan.FromDays(1);

    /// <summary>
    /// Gets the time interval to wait between executions of the <see cref="ReportSiteJob"/>.
    /// The delay is set to 5 minutes.
    /// </summary>
    public override TimeSpan Delay => TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets an array containing all possible values of the <see cref="ServerRole"/> enumeration.
    /// </summary>
    public override ServerRole[] ServerRoles => Enum.GetValues<ServerRole>();

    private readonly ILogger<ReportSiteJob> _logger;
    private readonly ITelemetryService _telemetryService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportSiteJob"/> class, responsible for reporting site telemetry data.
    /// </summary>
    /// <param name="logger">The logger used to record job execution details and errors.</param>
    /// <param name="telemetryService">The service used to collect and provide telemetry data for reporting.</param>
    /// <param name="jsonSerializer">The serializer used to convert telemetry data to JSON format for transmission.</param>
    /// <param name="httpClientFactory">The factory used to create HTTP clients for sending telemetry reports.</param>
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
    /// Executes the background job that sends the anonymous site ID to the telemetry service.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that is signaled when the host is shutting down.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    public override async Task RunJobAsync(CancellationToken cancellationToken)
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
            using (await httpClient.SendAsync(request, cancellationToken))
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
