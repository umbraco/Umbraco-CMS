using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

public class WebhookFiring : IRecurringBackgroundJob
{
    private readonly ILogger<WebhookFiring> _logger;
    private readonly IWebhookRequestService _webhookRequestService;
    private readonly IWebhookLogFactory _webhookLogFactory;
    private readonly IWebhookLogService _webhookLogService;
    private readonly IWebhookService _webHookService;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private WebhookSettings _webhookSettings;

    public TimeSpan Period => _webhookSettings.Period;

    public TimeSpan Delay { get; } = TimeSpan.FromSeconds(20);

    // No-op event as the period never changes on this job
    public event EventHandler PeriodChanged { add { } remove { } }

    public WebhookFiring(
        ILogger<WebhookFiring> logger,
        IWebhookRequestService webhookRequestService,
        IWebhookLogFactory webhookLogFactory,
        IWebhookLogService webhookLogService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        ICoreScopeProvider coreScopeProvider,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _webhookRequestService = webhookRequestService;
        _webhookLogFactory = webhookLogFactory;
        _webhookLogService = webhookLogService;
        _webHookService = webHookService;
        _coreScopeProvider = coreScopeProvider;
        _httpClientFactory = httpClientFactory;
        _webhookSettings = webhookSettings.CurrentValue;
        webhookSettings.OnChange(x => _webhookSettings = x);
    }

    public async Task RunJobAsync()
    {
        if (_webhookSettings.Enabled is false)
        {
            _logger.LogInformation("WebhookFiring task will not run as it has been globally disabled via configuration");
            return;
        }

        IEnumerable<WebhookRequest> requests;
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
        {
            scope.ReadLock(Constants.Locks.WebhookRequest);
            requests = await _webhookRequestService.GetAllAsync();
            scope.Complete();
        }

        await Task.WhenAll(requests.Select(request =>
        {
            using (ExecutionContext.SuppressFlow())
            {
                return Task.Run(async () =>
                {
                    IWebhook? webhook = await _webHookService.GetAsync(request.WebhookKey);
                    if (webhook is null)
                    {
                        return;
                    }

                    using HttpResponseMessage? response = await SendRequestAsync(webhook, request.EventAlias, request.RequestObject, request.RetryCount, CancellationToken.None);
                    if ((response?.IsSuccessStatusCode ?? false) || request.RetryCount >= _webhookSettings.MaximumRetries)
                    {
                        await _webhookRequestService.DeleteAsync(request);
                    }
                    else
                    {
                        request.RetryCount++;
                        await _webhookRequestService.UpdateAsync(request);
                    }
                });
            }
        }));
    }

    private async Task<HttpResponseMessage?> SendRequestAsync(IWebhook webhook, string eventName, string? serializedObject, int retryCount, CancellationToken cancellationToken)
    {
        using HttpClient httpClient = _httpClientFactory.CreateClient(Constants.HttpClients.WebhookFiring);

        using var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url)
        {
            Version = httpClient.DefaultRequestVersion,
            VersionPolicy = httpClient.DefaultVersionPolicy,
        };

        HttpResponseMessage? response = null;
        Exception? exception = null;
        try
        {
            // Add headers
            request.Headers.Add(Constants.WebhookEvents.HeaderNames.EventName, eventName);
            request.Headers.Add(Constants.WebhookEvents.HeaderNames.RetryCount, retryCount.ToString());

            foreach (KeyValuePair<string, string> header in webhook.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            // Set content
            request.Content = new StringContent(serializedObject ?? string.Empty, Encoding.UTF8, MediaTypeNames.Application.Json);

            // Send request
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
            _logger.LogError(ex, "Error while sending webhook request for webhook {WebhookKey}.", webhook.Key);
        }

        WebhookLog log = await _webhookLogFactory.CreateAsync(eventName, request, response, retryCount, exception, webhook, cancellationToken);
        await _webhookLogService.CreateAsync(log);

        return response;
    }
}
