using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Search.Configuration;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Umbraco.Search.DefferedActions;

/// <summary>
///     Removes all protected content from applicable indexes on a background thread
/// </summary>
internal class DeferredRemoveProtectedContent : IDeferredAction
{
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IScopeProvider _scopeProvider;
    private readonly IUmbracoIndexesConfiguration _configuration;
    private readonly IPublicAccessService _publicAccessService;
    private readonly ISearchProvider _searchProvider;

    public DeferredRemoveProtectedContent(IBackgroundTaskQueue backgroundTaskQueue,
        IScopeProvider scopeProvider,  IUmbracoIndexesConfiguration configuration,
        IPublicAccessService publicAccessService, ISearchProvider searchProvider)
    {
        _backgroundTaskQueue = backgroundTaskQueue;
        _scopeProvider = scopeProvider;
        _configuration = configuration;
        _publicAccessService = publicAccessService;
        _searchProvider = searchProvider;
    }

    public void Execute() => Execute(_backgroundTaskQueue, _scopeProvider, _configuration,
        _publicAccessService, _searchProvider);

    public static void Execute(IBackgroundTaskQueue backgroundTaskQueue,
        IScopeProvider scopeProvider,
        IUmbracoIndexesConfiguration umbracoIndexesConfiguration, IPublicAccessService publicAccessService,
        ISearchProvider searchProvider)
        => backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken =>
        {
            using ICoreScope scope =
                scopeProvider.CreateCoreScope(autoComplete: true);

            var protectedContentIds = publicAccessService.GetAll().Select(entry => entry.ProtectedNodeId).ToArray();
            if (protectedContentIds.Any() is false)
            {
                return Task.CompletedTask;
            }

            foreach (string indexName in searchProvider.GetAllIndexes())
            {
                var configuration = umbracoIndexesConfiguration.Configuration(indexName);
                if (!configuration.EnableDefaultEventHandler || !configuration.PublishedValuesOnly)
                {
                    continue;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

                var index = searchProvider.GetIndex<IContentBase>(indexName);
                if (index == null)
                {
                    continue;
                }

                index.RemoveFromIndex(protectedContentIds.Select(id => id.ToString()));
            }

            return Task.CompletedTask;
        });
}
