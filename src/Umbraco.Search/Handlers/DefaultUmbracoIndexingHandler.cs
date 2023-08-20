using System.Globalization;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using Umbraco.Search.Configuration;
using Umbraco.Search.DefferedActions;
using Umbraco.Search.NotificationHandlers;
using Umbraco.Search.Services;
using Umbraco.Search.DefferedActions.Indexing;
using Umbraco.Search.Models;

namespace Umbraco.Search.Examine;

/// <summary>
///     Indexing handler for Search provider indexes
/// </summary>
internal class DefaultUmbracoIndexingHandler : IUmbracoIndexingHandler
{
    // the default enlist priority is 100
    // enlist with a lower priority to ensure that anything "default" runs after us
    // but greater that SafeXmlReaderWriter priority which is 60
    private const int EnlistPriority = 80;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly Lazy<bool> _enabled;
    private readonly ISearchProvider _searchProvider;
    private readonly ILogger<DefaultUmbracoIndexingHandler> _logger;
    private readonly IMainDom _mainDom;
    private readonly IUmbracoIndexesConfiguration _configuration;
    private readonly IProfilingLogger _profilingLogger;
    private readonly IScopeProvider _scopeProvider;
    private readonly ISearchMainDomHandler _mainDomHandler;
    private readonly IPublicAccessService _publicAccessService;

    public DefaultUmbracoIndexingHandler(
        IMainDom mainDom,
        ILogger<DefaultUmbracoIndexingHandler> logger,
        IProfilingLogger profilingLogger,
        IScopeProvider scopeProvider,
        ISearchProvider searchProvider,
        IBackgroundTaskQueue backgroundTaskQueue,
        IUmbracoIndexesConfiguration configuration,
        ISearchMainDomHandler mainDomHandler, IPublicAccessService publicAccessService)

    {
        _mainDom = mainDom;
        _logger = logger;
        _profilingLogger = profilingLogger;
        _scopeProvider = scopeProvider;
        _searchProvider = searchProvider;
        _backgroundTaskQueue = backgroundTaskQueue;
        _configuration = configuration;
        _mainDomHandler = mainDomHandler;
        _publicAccessService = publicAccessService;
        _enabled = new Lazy<bool>(IsEnabled);
    }

    /// <inheritdoc />
    public bool Enabled => _enabled.Value;

    /// <inheritdoc />
    public void DeleteIndexForEntity(int entityId, bool keepIfUnpublished)
    {
        var actions = DeferredActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferedDeleteIndex(_searchProvider, _configuration, entityId, keepIfUnpublished));
        }
        else
        {
            DeferedDeleteIndex.Execute(_searchProvider, _configuration, entityId, keepIfUnpublished);
        }
    }

    /// <inheritdoc />
    public void DeleteIndexForEntities(IReadOnlyCollection<int> entityIds, bool keepIfUnpublished)
    {
        var actions = Umbraco.Search.DefferedActions.DeferredActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferedDeleteIndex(_searchProvider, _configuration, entityIds, keepIfUnpublished));
        }
        else
        {
            DeferedDeleteIndex.Execute(_searchProvider, _configuration, entityIds, keepIfUnpublished);
        }
    }

    /// <inheritdoc />
    public void ReIndexForContent(IContent sender, bool isPublished)
    {
        var actions = DeferredActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferedReIndexForContent(_backgroundTaskQueue, _configuration, _scopeProvider,
                _searchProvider, sender, isPublished));
        }
        else
        {
            DeferedReIndexForContent.Execute(_backgroundTaskQueue, _scopeProvider, _configuration, _searchProvider,
                sender, isPublished);
        }
    }

    /// <inheritdoc />
    public void ReIndexForMedia(IMedia sender, bool isPublished)
    {
        var actions = DeferredActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferedReIndexForMedia(_backgroundTaskQueue, _configuration, _scopeProvider,
                _searchProvider, sender, isPublished));
        }
        else
        {
            DeferedReIndexForMedia.Execute(_backgroundTaskQueue, _scopeProvider, _configuration, _searchProvider,
                sender, isPublished);
        }
    }

    /// <inheritdoc />
    public void RemoveProtectedContent()
    {
        var actions = DeferredActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferredRemoveProtectedContent(_backgroundTaskQueue, _scopeProvider, _configuration,
                _publicAccessService, _searchProvider));
        }
        else
        {
            DeferredRemoveProtectedContent.Execute(_backgroundTaskQueue, _scopeProvider, _configuration,
                _publicAccessService, _searchProvider);
        }
    }

    /// <inheritdoc />
    public void ReIndexForMember(IMember member)
    {
        var actions = DeferredActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(new DeferedReIndexForMember(_backgroundTaskQueue, _configuration, _searchProvider,
                _scopeProvider, member));
        }
        else
        {
            DeferedReIndexForMember.Execute(_backgroundTaskQueue, _configuration, _scopeProvider, _searchProvider,
                member);
        }
    }

    /// <inheritdoc />
    public void DeleteDocumentsForContentTypes(IReadOnlyCollection<int> removedContentTypes)
    {
        const int pageSize = 500;

        //Delete all content of this content/media/member type that is in any content indexer by looking up matched examine docs
        foreach (var id in removedContentTypes)
        {
            foreach (var indexName in _searchProvider.GetAllIndexes())
            {
                var page = 0;
                var total = long.MaxValue;
                var searcher = _searchProvider.GetSearcher(indexName);
                if (searcher == null)
                {
                    continue;
                }

                while (page * pageSize < total)
                {
                    //paging with examine, see https://shazwazza.com/post/paging-with-examine/
                    IUmbracoSearchResults results = searcher.Search(new DefaultSearchRequest(string.Empty,
                        new List<ISearchFilter>()
                        {
                            new DefaultSearchFilter("nodeType", new List<string>() { id.ToInvariantString() },
                                LogicOperator.And, new List<ISearchFilter>())
                        }, LogicOperator.And) { Page = page, PageSize = pageSize });


                    total = results.TotalRecords;
                    if (total <= 0 || results.Results == null || !results.Results.Any())
                    {
                        continue;
                    }

                    foreach (IUmbracoSearchResult item in results.Results)
                    {
                        if (int.TryParse(item.Id, NumberStyles.Integer, CultureInfo.InvariantCulture,
                                out var contentId))
                        {
                            DeleteIndexForEntity(contentId, false);
                        }
                    }

                    page++;
                }
            }
        }
    }

    /// <summary>
    ///     Used to lazily check if Examine Index handling is enabled
    /// </summary>
    /// <returns></returns>
    private bool IsEnabled()
    {
        //let's deal with shutting down Examine with MainDom
        var examineShutdownRegistered = _mainDom.Register(release: () =>
        {
            using (_profilingLogger.TraceDuration<DefaultUmbracoIndexingHandler>("Search provider shutting down"))
            {
                _searchProvider.Dispose();
            }
        });

        if (!examineShutdownRegistered)
        {
            _logger.LogInformation(
                "Search provider shutdown not registered, this AppDomain is not the MainDom, Search provider will be disabled");

            //if we could not register the shutdown examine ourselves, it means we are not maindom! in this case all of examine should be disabled!
            Suspendable.SearchIndexEvents.SuspendIndexers(_logger);
            return false; //exit, do not continue
        }

        _logger.LogDebug("Search provider shutdown registered with MainDom");

        var registeredIndexers =
            _searchProvider.GetAllIndexes()
                .Count(x => _configuration.Configuration(x).EnableDefaultEventHandler);

        _logger.LogInformation("Adding search provider event handlers for {RegisteredIndexers} index providers.",
            registeredIndexers);

        // don't bind event handlers if we're not suppose to listen
        if (registeredIndexers == 0)
        {
            return false;
        }

        return true;
    }
}
