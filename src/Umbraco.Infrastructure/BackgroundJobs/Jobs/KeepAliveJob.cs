// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
///     Hosted service implementation for keep alive feature.
/// </summary>
public class KeepAliveJob : IRecurringBackgroundJob
{
    public TimeSpan Period { get => TimeSpan.FromMinutes(5); }

    // No-op event as the period never changes on this job
    public event EventHandler PeriodChanged { add { } remove { } }

    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<KeepAliveJob> _logger;
    private readonly IProfilingLogger _profilingLogger;
    private KeepAliveSettings _keepAliveSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KeepAliveJob" /> class.
    /// </summary>
    /// <param name="hostingEnvironment">The current hosting environment</param>
    /// <param name="keepAliveSettings">The configuration for keep alive settings.</param>
    /// <param name="logger">The typed logger.</param>
    /// <param name="profilingLogger">The profiling logger.</param>
    /// <param name="httpClientFactory">Factory for <see cref="HttpClient" /> instances.</param>
    public KeepAliveJob(
        IHostingEnvironment hostingEnvironment,
        IOptionsMonitor<KeepAliveSettings> keepAliveSettings,
        ILogger<KeepAliveJob> logger,
        IProfilingLogger profilingLogger,
        IHttpClientFactory httpClientFactory)
    {
        _hostingEnvironment = hostingEnvironment;
        _keepAliveSettings = keepAliveSettings.CurrentValue;
        _logger = logger;
        _profilingLogger = profilingLogger;
        _httpClientFactory = httpClientFactory;

        keepAliveSettings.OnChange(x => _keepAliveSettings = x);
    }

    public async Task RunJobAsync()
    {
        if (_keepAliveSettings.DisableKeepAliveTask)
        {
            return;
        }

        using (_profilingLogger.DebugDuration<KeepAliveJob>("Keep alive executing", "Keep alive complete"))
        {
            var umbracoAppUrl = _hostingEnvironment.ApplicationMainUrl?.ToString();
            if (umbracoAppUrl.IsNullOrWhiteSpace())
            {
                _logger.LogWarning("No umbracoApplicationUrl for service (yet), skip.");
                return;
            }

            // If the config is an absolute path, just use it
            var keepAlivePingUrl = WebPath.Combine(
                umbracoAppUrl!,
                _hostingEnvironment.ToAbsolute(_keepAliveSettings.KeepAlivePingUrl));

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, keepAlivePingUrl);
                HttpClient httpClient = _httpClientFactory.CreateClient(Constants.HttpClients.IgnoreCertificateErrors);
                _ = await httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Keep alive failed (at '{keepAlivePingUrl}').", keepAlivePingUrl);
            }
        }
    }
}
