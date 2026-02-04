using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Implements <see cref="IWebhookLogService" /> to manage webhook log entries.
/// </summary>
public class WebhookLogService : IWebhookLogService
{
    private readonly IWebhookLogRepository _webhookLogRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebhookLogService" /> class.
    /// </summary>
    /// <param name="webhookLogRepository">The webhook log repository.</param>
    /// <param name="coreScopeProvider">The core scope provider.</param>
    public WebhookLogService(IWebhookLogRepository webhookLogRepository, ICoreScopeProvider coreScopeProvider)
    {
        _webhookLogRepository = webhookLogRepository;
        _coreScopeProvider = coreScopeProvider;
    }

    /// <inheritdoc />
    public async Task<WebhookLog> CreateAsync(WebhookLog webhookLog)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        await _webhookLogRepository.CreateAsync(webhookLog);
        scope.Complete();

        return webhookLog;
    }

    /// <inheritdoc />
    public async Task<PagedModel<WebhookLog>> Get(int skip = 0, int take = int.MaxValue)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
        return await _webhookLogRepository.GetPagedAsync(skip, take);
    }

    /// <inheritdoc />
    public async Task<PagedModel<WebhookLog>> Get(Guid webhookKey, int skip = 0, int take = int.MaxValue)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
        return await _webhookLogRepository.GetPagedAsync(webhookKey, skip, take);
    }
}
