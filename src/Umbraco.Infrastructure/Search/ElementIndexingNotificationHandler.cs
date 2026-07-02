using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Search;

/// <summary>
/// Queues reindexing of documents that reference elements when those elements are saved or published.
/// </summary>
internal sealed class ElementIndexingNotificationHandler :
    INotificationHandler<ElementSavedNotification>,
    INotificationHandler<ElementPublishedNotification>
{
    private readonly IDeferredSearchReindexService _deferredSearchReindexService;

    public ElementIndexingNotificationHandler(IDeferredSearchReindexService deferredSearchReindexService)
        => _deferredSearchReindexService = deferredSearchReindexService;

    public void Handle(ElementSavedNotification notification)
        => QueueElementReindex(notification.SavedEntities);

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
