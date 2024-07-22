using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Handlers;

public class RoutingNotificationHandler : INotificationAsyncHandler<ContentPublishedNotification>
{
    private readonly IDocumentUrlService _documentUrlService;

    public RoutingNotificationHandler(IDocumentUrlService documentUrlService) => _documentUrlService = documentUrlService;

    public async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken) => await _documentUrlService.CreateOrUpdateUrlSegmentsAsync(notification.PublishedEntities);
}
