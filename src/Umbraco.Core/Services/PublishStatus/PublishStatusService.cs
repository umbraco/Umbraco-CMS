using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Navigation;

public class PublishStatusService : IPublishStatusManagementService, IPublishStatusQueryService
{
    private readonly ILogger<PublishStatusService> _logger;
    private readonly IPublishStatusRepository _publishStatusRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private static IDictionary<Guid, ISet<string>> _publishedCultures = new Dictionary<Guid, ISet<string>>();
    public PublishStatusService(
        ILogger<PublishStatusService> logger,
        IPublishStatusRepository publishStatusRepository,
        ICoreScopeProvider coreScopeProvider)
    {
        _logger = logger;
        _publishStatusRepository = publishStatusRepository;
        _coreScopeProvider = coreScopeProvider;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _publishedCultures.Clear();
        IDictionary<Guid, ISet<string>> publishStatus;
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
        {
            publishStatus = await _publishStatusRepository.GetAllPublishStatusAsync(cancellationToken);
            scope.Complete();
        }

        foreach ((Guid documentKey, ISet<string> publishedCultures) in publishStatus)
        {
            if (_publishedCultures.TryAdd(documentKey, publishedCultures) is false)
            {
                _logger.LogWarning("Failed to add published cultures for document {DocumentKey}", documentKey);
            }
        }


    }

    public bool IsDocumentPublished(Guid documentKey, string culture)
    {
        if (_publishedCultures.TryGetValue(documentKey, out ISet<string>? publishedCultures))
        {
            return publishedCultures.Contains(culture);
        }

        _logger.LogDebug("Document {DocumentKey} not found in the publish status cache", documentKey);
        return false;
    }

    public async Task AddOrUpdateStatusAsync(Guid documentKey, CancellationToken cancellationToken)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        ISet<string> publishedCultures = await _publishStatusRepository.GetPublishStatusAsync(documentKey, cancellationToken);
        _publishedCultures[documentKey] = publishedCultures;
        scope.Complete();
    }

    public Task RemoveAsync(Guid documentKey, CancellationToken cancellationToken)
    {
        _publishedCultures.Remove(documentKey);
        return Task.CompletedTask;
    }

    public async Task AddOrUpdateStatusWithDescendantsAsync(Guid rootDocumentKey, CancellationToken cancellationToken)
    {
        IDictionary<Guid, ISet<string>> publishStatus;
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
        {
            publishStatus = await _publishStatusRepository.GetDescendantsOrSelfPublishStatusAsync(rootDocumentKey, cancellationToken);
            scope.Complete();
        }

        foreach ((Guid documentKey, ISet<string> publishedCultures) in publishStatus)
        {
            _publishedCultures[documentKey] = publishedCultures;
        }
    }
}
