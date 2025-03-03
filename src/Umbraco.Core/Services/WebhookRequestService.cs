using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Services;

public class WebhookRequestService : IWebhookRequestService
{
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IWebhookRequestRepository _webhookRequestRepository;
    private readonly IWebhookJsonSerializer _webhookJsonSerializer;

    [Obsolete("This constructor is obsolete and will be removed in future versions. Scheduled for removal in V17")]
    public WebhookRequestService(ICoreScopeProvider coreScopeProvider, IWebhookRequestRepository webhookRequestRepository, IJsonSerializer jsonSerializer)
    : this (coreScopeProvider, webhookRequestRepository, StaticServiceProvider.Instance.GetRequiredService<IWebhookJsonSerializer>())
    {
    }

    public WebhookRequestService(ICoreScopeProvider coreScopeProvider, IWebhookRequestRepository webhookRequestRepository, IWebhookJsonSerializer webhookJsonSerializer)
    {
        _coreScopeProvider = coreScopeProvider;
        _webhookRequestRepository = webhookRequestRepository;
        _webhookJsonSerializer = webhookJsonSerializer;
    }

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
        scope.WriteLock(Constants.Locks.WebhookRequest);
        await _webhookRequestRepository.DeleteAsync(webhookRequest);
        scope.Complete();
    }

    public async Task<WebhookRequest> UpdateAsync(WebhookRequest webhookRequest)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.WebhookRequest);
        WebhookRequest updated = await _webhookRequestRepository.UpdateAsync(webhookRequest);
        scope.Complete();
        return updated;
    }
}
