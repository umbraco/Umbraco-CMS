using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Search;

/// <summary>
/// Queues reindexing of documents that reference elements when those elements are published.
/// </summary>
/// <remarks>
/// Only published element values are flattened into the (published) external index, so a draft save cannot change
/// what referencing documents index. Reindexing is therefore triggered on publish only, not on save.
/// </remarks>
internal sealed class ElementIndexingNotificationHandler :
    INotificationHandler<ElementPublishedNotification>
{
    private readonly IDeferredSearchReindexService _deferredSearchReindexService;

    public ElementIndexingNotificationHandler(IDeferredSearchReindexService deferredSearchReindexService)
        => _deferredSearchReindexService = deferredSearchReindexService;

    public void Handle(ElementPublishedNotification notification)
        => QueueElementReindex(notification.PublishedEntities);

    private void QueueElementReindex(IEnumerable<IElement> elements)
    {
        var ids = elements.Select(e => e.Id).ToArray();
        if (ids.Length > 0)
        {
            _deferredSearchReindexService.QueueElementReindex(ids);
        }
    }
}
