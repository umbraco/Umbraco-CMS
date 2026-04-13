using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.Examine.Deferred;
using Umbraco.Cms.Infrastructure.Search;

namespace Umbraco.Cms.Infrastructure.Examine;

internal sealed class DeliveryApiIndexingHandler : IDeliveryApiIndexingHandler
{
    // these are the dependencies for this handler
    private readonly ExamineIndexingMainDomHandler _mainDomHandler;
    private readonly IExamineManager _examineManager;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly ILogger<DeliveryApiIndexingHandler> _logger;
    private DeliveryApiSettings _deliveryApiSettings;
    private readonly Lazy<bool> _enabled;

    // these dependencies are for the deferred handling (we don't want those handlers registered in the DI)
    private readonly IContentService _contentService;
    private readonly IPublicAccessService _publicAccessService;
    private readonly IDeliveryApiContentIndexValueSetBuilder _deliveryApiContentIndexValueSetBuilder;
    private readonly IDeliveryApiContentIndexHelper _deliveryApiContentIndexHelper;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IDeliveryApiCompositeIdHandler _deliveryApiCompositeIdHandler;


    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Examine.DeliveryApiIndexingHandler"/> class, responsible for handling Examine indexing for the Delivery API.
    /// </summary>
    /// <param name="mainDomHandler">Handles main domain (MainDom) events for Examine indexing coordination.</param>
    /// <param name="examineManager">Manages Examine indexes and searchers.</param>
    /// <param name="scopeProvider">Provides database scope management for transactional operations.</param>
    /// <param name="logger">The logger used for logging indexing operations and errors.</param>
    /// <param name="deliveryApiSettings">Monitors configuration settings for the Delivery API.</param>
    /// <param name="contentService">Service for accessing and managing Umbraco content items.</param>
    /// <param name="publicAccessService">Service for managing public access permissions on content.</param>
    /// <param name="deliveryApiContentIndexValueSetBuilder">Builds value sets for content items to be indexed for the Delivery API.</param>
    /// <param name="deliveryApiContentIndexHelper">Provides helper methods for content indexing specific to the Delivery API.</param>
    /// <param name="backgroundTaskQueue">Queue for scheduling background indexing tasks.</param>
    /// <param name="deliveryApiCompositeIdHandler">Handles composite IDs for Delivery API content indexing.</param>
    public DeliveryApiIndexingHandler(
        ExamineIndexingMainDomHandler mainDomHandler,
        IExamineManager examineManager,
        ICoreScopeProvider scopeProvider,
        ILogger<DeliveryApiIndexingHandler> logger,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings,
        IContentService contentService,
        IPublicAccessService publicAccessService,
        IDeliveryApiContentIndexValueSetBuilder deliveryApiContentIndexValueSetBuilder,
        IDeliveryApiContentIndexHelper deliveryApiContentIndexHelper,
        IBackgroundTaskQueue backgroundTaskQueue,
        IDeliveryApiCompositeIdHandler deliveryApiCompositeIdHandler)
    {
        _mainDomHandler = mainDomHandler;
        _examineManager = examineManager;
        _scopeProvider = scopeProvider;
        _logger = logger;
        _contentService = contentService;
        _publicAccessService = publicAccessService;
        _deliveryApiContentIndexValueSetBuilder = deliveryApiContentIndexValueSetBuilder;
        _deliveryApiContentIndexHelper = deliveryApiContentIndexHelper;
        _backgroundTaskQueue = backgroundTaskQueue;
        _deliveryApiCompositeIdHandler = deliveryApiCompositeIdHandler;

        _enabled = new Lazy<bool>(IsEnabled);
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
    }

    /// <inheritdoc />
    public bool Enabled => _enabled.Value;

    /// <inheritdoc />
    public void HandleContentChanges(IList<KeyValuePair<int, TreeChangeTypes>> changes)
    {
        var deferred = new DeliveryApiContentIndexHandleContentChanges(
            changes,
            this,
            _contentService,
            _deliveryApiContentIndexValueSetBuilder,
            _deliveryApiContentIndexHelper,
            _backgroundTaskQueue);
        Execute(deferred);
    }

    /// <inheritdoc />
    public void HandleContentTypeChanges(IList<KeyValuePair<int, ContentTypeChangeTypes>> changes)
    {
        var deferred = new DeliveryApiContentIndexHandleContentTypeChanges(
            changes,
            this,
            _deliveryApiContentIndexValueSetBuilder,
            _contentService,
            _backgroundTaskQueue,
            _deliveryApiCompositeIdHandler);
        Execute(deferred);
    }

    /// <inheritdoc />
    public void HandlePublicAccessChanges()
    {
        var deferred = new DeliveryApiContentIndexHandlePublicAccessChanges(
            _publicAccessService,
            this,
            _contentService,
            _deliveryApiContentIndexValueSetBuilder,
            _deliveryApiContentIndexHelper,
            _deliveryApiSettings,
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

    internal IIndex? GetIndex()
        => _examineManager.TryGetIndex(Constants.UmbracoIndexes.DeliveryApiContentIndexName, out IIndex index)
            ? index
            : null;
}
