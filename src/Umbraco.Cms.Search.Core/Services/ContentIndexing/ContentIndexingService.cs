using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Configuration;
using Umbraco.Cms.Search.Core.Models.Configuration;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

internal sealed class ContentIndexingService : IContentIndexingService
{
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger<ContentIndexingService> _logger;
    private readonly IndexOptions _indexOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOriginProvider _originProvider;

    public ContentIndexingService(
        IBackgroundTaskQueue backgroundTaskQueue,
        IEventAggregator eventAggregator,
        ILogger<ContentIndexingService> logger,
        IOptions<IndexOptions> indexOptions,
        IServiceProvider serviceProvider,
        IOriginProvider originProvider)
    {
        _backgroundTaskQueue = backgroundTaskQueue;
        _eventAggregator = eventAggregator;
        _logger = logger;
        _indexOptions = indexOptions.Value;
        _serviceProvider = serviceProvider;
        _originProvider = originProvider;
    }

    public void Handle(IEnumerable<ContentChange> changes, string origin)
    {
        ContentChange[] changesAsArray = changes as ContentChange[] ?? changes.ToArray();

        var currentOrigin = _originProvider.GetCurrent();
        IEnumerable<IGrouping<Type, ContentIndexRegistration>> indexRegistrationsByStrategyType = _indexOptions
            .GetContentIndexRegistrations()
            .Where(registration => registration.SameOriginOnly is false || origin == currentOrigin)
            .GroupBy(r => r.ContentChangeStrategy);

        foreach (IGrouping<Type, ContentIndexRegistration> group in indexRegistrationsByStrategyType)
        {
            if (TryGetContentChangeStrategy(group.Key, out IContentChangeStrategy? contentChangeStrategy) is false)
            {
                continue;
            }

            ContentIndexInfo[] indexInfos = group
                .Select(g =>
                    TryGetIndexer(g.Indexer, out IIndexer? indexer)
                        ? new ContentIndexInfo(g.IndexAlias, g.ContainedObjectTypes, indexer)
                        : null)
                .WhereNotNull()
                .ToArray();

            if (indexInfos.Length == 0)
            {
                _logger.LogWarning($"Could not resolve any indexes for {nameof(IContentChangeStrategy)} of type {{type}}. Index updates will be skipped.", group.Key.FullName);
                continue;
            }

            _backgroundTaskQueue.QueueBackgroundWorkItem(async cancellationToken => await contentChangeStrategy.HandleAsync(indexInfos, changesAsArray, cancellationToken));
        }
    }

    public void Rebuild(string indexAlias, string origin)
    {
        ContentIndexRegistration? indexRegistration = _indexOptions.GetContentIndexRegistration(indexAlias);
        if (indexRegistration is null)
        {
            _logger.LogError("Cannot rebuild index - no index registration found for alias: {indexAlias}", indexAlias);
            return;
        }

        if (indexRegistration.SameOriginOnly && origin != _originProvider.GetCurrent())
        {
            return;
        }

        _backgroundTaskQueue.QueueBackgroundWorkItem(async cancellationToken => await RebuildAsync(indexRegistration, cancellationToken));
    }

    private async Task RebuildAsync(ContentIndexRegistration indexRegistration, CancellationToken cancellationToken)
    {
        if (TryGetContentChangeStrategy(indexRegistration.ContentChangeStrategy, out IContentChangeStrategy? contentChangeStrategy) is false
            || TryGetIndexer(indexRegistration.Indexer, out IIndexer? indexer) is false)
        {
            return;
        }

        await _eventAggregator.PublishAsync(new IndexRebuildStartingNotification(indexRegistration.IndexAlias), cancellationToken);

        await contentChangeStrategy.RebuildAsync(new ContentIndexInfo(indexRegistration.IndexAlias, indexRegistration.ContainedObjectTypes, indexer), cancellationToken);

        await _eventAggregator.PublishAsync(new IndexRebuildCompletedNotification(indexRegistration.IndexAlias), cancellationToken);
    }

    private bool TryGetContentChangeStrategy(Type type, [NotNullWhen(true)] out IContentChangeStrategy? contentChangeStrategy)
    {
        if (_serviceProvider.GetService(type) is IContentChangeStrategy resolvedContentChangeStrategy)
        {
            contentChangeStrategy = resolvedContentChangeStrategy;
            return true;
        }

        _logger.LogError($"Could not resolve type {{type}} as {nameof(IContentChangeStrategy)}. Make sure the type is registered in the DI.", type.FullName);
        contentChangeStrategy = null;
        return false;
    }

    private bool TryGetIndexer(Type type, [NotNullWhen(true)] out IIndexer? indexer)
    {
        if (_serviceProvider.GetService(type) is IIndexer resolvedIndexer)
        {
            indexer = resolvedIndexer;
            return true;
        }

        _logger.LogError($"Could not resolve type {{type}} as {nameof(IIndexer)}. Make sure the type is registered in the DI.", type.FullName);
        indexer = null;
        return false;
    }
}
