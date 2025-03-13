using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Core.Services;

public class WebhookLogService : IWebhookLogService
{
    private readonly IWebhookLogRepository _webhookLogRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;

    public WebhookLogService(IWebhookLogRepository webhookLogRepository, ICoreScopeProvider coreScopeProvider)
    {
        _webhookLogRepository = webhookLogRepository;
        _coreScopeProvider = coreScopeProvider;
    }

    public async Task<WebhookLog> CreateAsync(WebhookLog webhookLog)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        await _webhookLogRepository.CreateAsync(webhookLog);
        scope.Complete();

        return webhookLog;
    }

    public async Task<PagedModel<WebhookLog>> Get(int skip = 0, int take = int.MaxValue)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
        return await _webhookLogRepository.GetPagedAsync(skip, take);
    }
}
