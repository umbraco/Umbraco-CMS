using Examine;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Extensions;
using Umbraco.Search.Configuration;
using Umbraco.Search.ValueSet.ValueSetBuilders;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Umbraco.Search.DefferedActions.Indexing;

/// <summary>
///     Re-indexes an <see cref="IMember" /> item on a background thread
/// </summary>
internal partial class DeferedReIndexForMember : IDeferredAction
{
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IUmbracoIndexesConfiguration _configuration;
    private readonly ISearchProvider _searchProvider;
    private readonly IScopeProvider _scopeProvider;
    private readonly IMember _member;

    public DeferedReIndexForMember(IBackgroundTaskQueue backgroundTaskQueue,
        IUmbracoIndexesConfiguration configuration,
        ISearchProvider searchProvider, IScopeProvider scopeProvider,  IMember member)
    {
        _member = member;
        _backgroundTaskQueue = backgroundTaskQueue;
        _configuration = configuration;
        _searchProvider = searchProvider;
        _scopeProvider = scopeProvider;
    }

    public void Execute() => Execute(_backgroundTaskQueue, _configuration,_scopeProvider,_searchProvider, _member);

    public static void Execute(IBackgroundTaskQueue backgroundTaskQueue,
        IUmbracoIndexesConfiguration umbracoIndexesConfiguration,
        IScopeProvider scopeProvider,
        ISearchProvider searchProvider, IMember member) =>
        // perform the ValueSet lookup on a background thread
        backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken =>
        {
            using ICoreScope scope =
                scopeProvider.CreateCoreScope(autoComplete: true);

            foreach (string indexName in searchProvider.GetAllIndexes())
            {
                var configuration = umbracoIndexesConfiguration.Configuration(indexName);
                if (!configuration.EnableDefaultEventHandler)
                {
                    continue;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

                var index = searchProvider.GetIndex<IMember>(indexName);
                if (index == null)
                {
                    continue;
                }

                index.IndexItems(member.AsEnumerableOfOne().ToArray());
            }

            return Task.CompletedTask;
        });
}
