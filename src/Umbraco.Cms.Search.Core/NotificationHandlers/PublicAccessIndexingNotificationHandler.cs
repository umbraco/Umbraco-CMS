using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Search.Core.Cache.PublicAccess;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.NotificationHandlers;

/// <summary>
/// Re-indexes published content affected by public access (protection) changes.
/// </summary>
internal sealed class PublicAccessIndexingNotificationHandler : IndexingNotificationHandlerBase, INotificationAsyncHandler<PublicAccessDetailedCacheRefresherNotification>
{
    private readonly IContentIndexingService _contentIndexingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicAccessIndexingNotificationHandler"/> class.
    /// </summary>
    /// <param name="coreScopeProvider">The scope provider used to defer actions until the ambient scope completes.</param>
    /// <param name="contentIndexingService">The service used to re-index the affected content.</param>
    public PublicAccessIndexingNotificationHandler(ICoreScopeProvider coreScopeProvider, IContentIndexingService contentIndexingService)
        : base(coreScopeProvider)
        => _contentIndexingService = contentIndexingService;

    /// <summary>
    /// Re-indexes the content whose public access protection changed, including descendants.
    /// </summary>
    /// <param name="notification">The notification describing the public access change to react to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task HandleAsync(PublicAccessDetailedCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        PublicAccessDetailedCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads<PublicAccessDetailedCacheRefresher.JsonPayload>(notification, out var origin);
        ContentChange[] changes = payloads
            .Select(payload => ContentChange.Document(payload.ProtectedContentKey, ChangeImpact.RefreshWithDescendants, ContentState.Published))
            .ToArray();

        ExecuteDeferred(() => _contentIndexingService.Handle(changes, origin));

        return Task.CompletedTask;
    }
}
