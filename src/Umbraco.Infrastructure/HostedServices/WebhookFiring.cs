using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.HostedServices;

public class WebhookFiring : RecurringHostedServiceBase
{
    private readonly ILogger<WebhookFiring> _logger;
    private readonly IRuntimeState _runtimeState;
    private readonly IWebhookRequestService _webhookRequestService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IWebhookLogFactory _webhookLogFactory;
    private readonly IWebhookLogService _webhookLogService;
    private readonly IWebHookService _webHookService;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private WebhookSettings _webhookSettings;

    public WebhookFiring(
        ILogger<WebhookFiring> logger,
        IRuntimeState runtimeState,
        IWebhookRequestService webhookRequestService,
        IJsonSerializer jsonSerializer,
        IWebhookLogFactory webhookLogFactory,
        IWebhookLogService webhookLogService,
        IWebHookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        ICoreScopeProvider coreScopeProvider)
        : base(logger, TimeSpan.FromSeconds(10), DefaultDelay)
    {
        _logger = logger;
        _runtimeState = runtimeState;
        _webhookRequestService = webhookRequestService;
        _jsonSerializer = jsonSerializer;
        _webhookLogFactory = webhookLogFactory;
        _webhookLogService = webhookLogService;
        _webHookService = webHookService;
        _coreScopeProvider = coreScopeProvider;
        _webhookSettings = webhookSettings.CurrentValue;
        webhookSettings.OnChange(x => _webhookSettings = x);
    }

    public override async Task PerformExecuteAsync(object? state)
    {
        // Don't process webhooks if we're not in RuntimeLevel.Run.
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Does not run if run level is not Run.");
            }

            return;
        }

        await ProcessWebhookRequests();
    }

    private async Task ProcessWebhookRequests()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.WebhookRequest);
        IEnumerable<WebhookRequest> requests = await _webhookRequestService.GetAllAsync();
        foreach (WebhookRequest request in requests)
        {
            Webhook? webhook = await _webHookService.GetAsync(request.WebhookKey);
            if (webhook is null)
            {
                continue;
            }

            HttpResponseMessage? response = await SendRequestAsync(webhook, request.EventAlias, request.RequestObject, request.RetryCount, CancellationToken.None);

            if ((response?.IsSuccessStatusCode ?? false) || request.RetryCount >= _webhookSettings.MaximumRetries)
            {
                await _webhookRequestService.DeleteAsync(request);
            }
            else
            {
                request.RetryCount++;
                await _webhookRequestService.UpdateAsync(request);
            }
        }

        scope.Complete();
    }

    private async Task<HttpResponseMessage?> SendRequestAsync(Webhook webhook, string eventName, object? payload, int retryCount, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();

        var serializedObject = _jsonSerializer.Serialize(payload);
        var stringContent = new StringContent(serializedObject, Encoding.UTF8, "application/json");
        stringContent.Headers.TryAddWithoutValidation("Umb-Webhook-Event", eventName);

        foreach (KeyValuePair<string, string> header in webhook.Headers)
        {
            stringContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        HttpResponseMessage? response = null;
        try
        {
            response = await httpClient.PostAsync(webhook.Url, stringContent, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending webhook request for webhook {WebhookKey}.", webhook);
        }

        var webhookResponseModel = new WebhookResponseModel
        {
            HttpResponseMessage = response,
            RetryCount = retryCount,
        };


        WebhookLog log = await _webhookLogFactory.CreateAsync(eventName, webhookResponseModel, webhook, cancellationToken);
        await _webhookLogService.CreateAsync(log);

        return response;
    }
}
