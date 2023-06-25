using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Search.DefferedActions;
using Umbraco.Search.Examine.ValueSetBuilders;
using Umbraco.Search.Services;

namespace Umbraco.Search;

internal sealed class DeliveryApiIndexingHandler : IDeliveryApiIndexingHandler
{
    // these are the dependencies for this handler
    private readonly ISearchMainDomHandler _mainDomHandler;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly ISearchProvider _searchProvider;
    private readonly ILogger<DeliveryApiIndexingHandler> _logger;
    private readonly Lazy<bool> _enabled;

    // these dependencies are for the deferred handling (we don't want those handlers registered in the DI)
    private readonly IContentService _contentService;
    private readonly IPublicAccessService _publicAccessService;
    private readonly IDeliveryApiContentIndexHelper _deliveryApiContentIndexHelper;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public DeliveryApiIndexingHandler(
        ISearchMainDomHandler mainDomHandler,
        ICoreScopeProvider scopeProvider,
        ISearchProvider searchProvider,
        ILogger<DeliveryApiIndexingHandler> logger,
        IContentService contentService,
        IPublicAccessService publicAccessService,
        IDeliveryApiContentIndexHelper deliveryApiContentIndexHelper,
        IBackgroundTaskQueue backgroundTaskQueue)
    {
        _mainDomHandler = mainDomHandler;
        _scopeProvider = scopeProvider;
        _searchProvider = searchProvider;
        _logger = logger;
        _contentService = contentService;
        _publicAccessService = publicAccessService;
        _deliveryApiContentIndexHelper = deliveryApiContentIndexHelper;
        _backgroundTaskQueue = backgroundTaskQueue;
        _enabled = new Lazy<bool>(IsEnabled);
    }

    /// <inheritdoc />
    public bool Enabled => _enabled.Value;

    /// <inheritdoc />
    public void HandleContentChanges(IList<KeyValuePair<int, TreeChangeTypes>> changes)
    {
        var deferred = new DeliveryApiContentIndexHandleContentChanges(
            changes,
            this,
            _deliveryApiContentIndexHelper,
            _contentService,
_searchProvider,
            _backgroundTaskQueue);
        Execute(deferred);
    }

    /// <inheritdoc />
    public void HandleContentTypeChanges(IList<KeyValuePair<int, ContentTypeChangeTypes>> changes)
    {
        var deferred = new DeliveryApiContentIndexHandleContentTypeChanges(
            changes,
            this,
            _contentService,
            _searchProvider,
            _backgroundTaskQueue);
        Execute(deferred);
    }

    /// <inheritdoc />
    public void HandlePublicAccessChanges()
    {
        var deferred = new DeliveryApiContentIndexHandlePublicAccessChanges(
            _publicAccessService,
            this,
            _searchProvider,
            _backgroundTaskQueue);
        Execute(deferred);
    }

    private void Execute(IDeferredAction action)
    {
        var actions = DeferredActions.Get(_scopeProvider);
        if (actions != null)
        {
            actions.Add(action);
        }
        else
        {
            action.Execute();
        }
    }

    private bool IsEnabled()
    {
        if (_mainDomHandler.IsMainDom() == false)
        {
            return false;
        }

        if (GetIndex() is null)
        {
            _logger.LogInformation("The Delivery API content index could not be found, Examine indexing is disabled.");
            return false;
        }

        return true;
    }

    internal IUmbracoIndex<IContent>? GetIndex()
        => _searchProvider.GetIndex<IContent>(Constants.UmbracoIndexes.DeliveryApiContentIndexName);

    public IUmbracoSearcher? GetSearcher() =>
        _searchProvider.GetSearcher(Constants.UmbracoIndexes.DeliveryApiContentIndexName);
}
