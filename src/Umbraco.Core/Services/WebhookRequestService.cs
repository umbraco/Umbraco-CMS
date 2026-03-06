using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides functionality for managing webhook requests.
/// </summary>
/// <remarks>
///     Webhook requests are queued when events occur and are processed asynchronously
///     to send HTTP callbacks to configured webhook endpoints.
/// </remarks>
public class WebhookRequestService : IWebhookRequestService
{
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IWebhookRequestRepository _webhookRequestRepository;
    private readonly IWebhookJsonSerializer _webhookJsonSerializer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebhookRequestService" /> class.
    /// </summary>
    /// <param name="coreScopeProvider">The scope provider for unit of work operations.</param>
    /// <param name="webhookRequestRepository">The repository for webhook request data access.</param>
    /// <param name="webhookJsonSerializer">The serializer for converting payloads to JSON.</param>
    public WebhookRequestService(ICoreScopeProvider coreScopeProvider, IWebhookRequestRepository webhookRequestRepository, IWebhookJsonSerializer webhookJsonSerializer)
    {
        _coreScopeProvider = coreScopeProvider;
        _webhookRequestRepository = webhookRequestRepository;
        _webhookJsonSerializer = webhookJsonSerializer;
    }

    /// <inheritdoc />
    public async Task<WebhookRequest> CreateAsync(Guid webhookKey, string eventAlias, object? payload)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.WebhookRequest);
        var webhookRequest = new WebhookRequest
        {
            WebhookKey = webhookKey,
            EventAlias = eventAlias,
            RequestObject = _webhookJsonSerializer.Serialize(payload),
            RetryCount = 0,
        };

        webhookRequest = await _webhookRequestRepository.CreateAsync(webhookRequest);
        scope.Complete();

        return webhookRequest;
    }

    /// <inheritdoc />
    public Task<IEnumerable<WebhookRequest>> GetAllAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        Task<IEnumerable<WebhookRequest>> webhookRequests = _webhookRequestRepository.GetAllAsync();
        scope.Complete();
        return webhookRequests;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(WebhookRequest webhookRequest)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.WebhookRequest);
        await _webhookRequestRepository.DeleteAsync(webhookRequest);
        scope.Complete();
    }

    /// <inheritdoc />
    public async Task<WebhookRequest> UpdateAsync(WebhookRequest webhookRequest)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.WebhookRequest);
        WebhookRequest updated = await _webhookRequestRepository.UpdateAsync(webhookRequest);
        scope.Complete();
        return updated;
    }
}
