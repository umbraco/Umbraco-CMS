using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Services;

public class WebhookRequestService : IWebhookRequestService
{
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IWebhookRequestRepository _webhookRequestRepository;
    private readonly IJsonSerializer _jsonSerializer;

    public WebhookRequestService(ICoreScopeProvider coreScopeProvider, IWebhookRequestRepository webhookRequestRepository, IJsonSerializer jsonSerializer)
    {
        _coreScopeProvider = coreScopeProvider;
        _webhookRequestRepository = webhookRequestRepository;
        _jsonSerializer = jsonSerializer;
    }

    public async Task<WebhookRequest> CreateAsync(Guid webhookKey, string eventName, object? payload)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        var webhookRequest = new WebhookRequest
        {
            WebhookKey = webhookKey,
            EventName = eventName,
            RequestObject = _jsonSerializer.Serialize(payload),
            RetryCount = 0,
        };

        webhookRequest = await _webhookRequestRepository.CreateAsync(webhookRequest);
        scope.Complete();

        return webhookRequest;
    }

    public Task<IEnumerable<WebhookRequest>> GetAllAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        Task<IEnumerable<WebhookRequest>> webhookRequests = _webhookRequestRepository.GetAllAsync();
        scope.Complete();
        return webhookRequests;
    }

    public async Task DeleteAsync(WebhookRequest webhookRequest)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        await _webhookRequestRepository.DeleteAsync(webhookRequest);
        scope.Complete();
    }

    public async Task<WebhookRequest> UpdateAsync(WebhookRequest webhookRequest)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        WebhookRequest updated = await _webhookRequestRepository.UpdateAsync(webhookRequest);
        scope.Complete();
        return updated;
    }
}
