using Examine;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Extensions;
using Umbraco.Search.Configuration;
using Umbraco.Search.Examine;
using Umbraco.Search.ValueSet.Validators;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Umbraco.Search.DefferedActions.Indexing;

/// <summary>
///     Re-indexes an <see cref="IMedia" /> item on a background thread
/// </summary>
internal class DeferedReIndexForMedia : IDeferredAction
{
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IUmbracoIndexesConfiguration _configuration;
    private readonly IScopeProvider _provider;
    private readonly ISearchProvider _searchProvide;
    private readonly bool _isPublished;
    private readonly IMedia _media;

    public DeferedReIndexForMedia(IBackgroundTaskQueue backgroundTaskQueue,
        IUmbracoIndexesConfiguration configuration, IScopeProvider provider, ISearchProvider searchProvide,
      IMedia media, bool isPublished)
    {
        _backgroundTaskQueue = backgroundTaskQueue;
        _configuration = configuration;
        _provider = provider;
        _searchProvide = searchProvide;
        _media = media;
        _isPublished = isPublished;
    }

    public void Execute() =>
        Execute(_backgroundTaskQueue,_provider, _configuration, _searchProvide,  _media, _isPublished);

    public static void Execute(IBackgroundTaskQueue backgroundTaskQueue, IScopeProvider provider,
        IUmbracoIndexesConfiguration umbracoIndexesConfiguration, ISearchProvider searchProvider,
        IMedia media, bool isPublished) =>
        // perform the ValueSet lookup on a background thread
        backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken =>
        {
            using ICoreScope scope =
                provider.CreateCoreScope(autoComplete: true);


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

                index.IndexItems(media.AsEnumerableOfOne().ToArray());
            }

            return Task.CompletedTask;
        });
}
