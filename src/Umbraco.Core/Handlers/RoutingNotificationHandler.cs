using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Handlers;

public class RoutingNotificationHandler
    : INotificationAsyncHandler<ContentPublishedNotification>,
        INotificationAsyncHandler<ContentDeletedNotification>,
        INotificationAsyncHandler<ContentUnpublishedNotification>
{
    private readonly IDocumentUrlService _documentUrlService;

    public RoutingNotificationHandler(IDocumentUrlService documentUrlService) => _documentUrlService = documentUrlService;

    public async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken) => await _documentUrlService.CreateOrUpdateUrlSegmentsAsync(notification.PublishedEntities);
    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken) => await _documentUrlService.DeleteUrlsAsync(notification.DeletedEntities);
    public async Task HandleAsync(ContentUnpublishedNotification notification, CancellationToken cancellationToken) => await _documentUrlService.DeleteUrlsAsync(notification.UnpublishedEntities);
}
