// using Umbraco.Cms.Core.Events;
// using Umbraco.Cms.Core.Models;
// using Umbraco.Cms.Core.Notifications;
// using Umbraco.Cms.Core.Services;
//
// namespace Umbraco.Cms.Core.Handlers;
//
// public class RoutingNotificationHandler
//     : INotificationAsyncHandler<ContentPublishedNotification>,
//         INotificationAsyncHandler<ContentDeletedNotification>,
//         INotificationAsyncHandler<ContentUnpublishedNotification>,
//         INotificationAsyncHandler<ContentSavedNotification>
// {
//     private readonly IDocumentUrlService _documentUrlService;
//     private readonly IContentService _contentService;
//     private readonly IIdKeyMap _idKeyMap;
//
//     public RoutingNotificationHandler(IDocumentUrlService documentUrlService, IContentService contentService, IIdKeyMap idKeyMap)
//     {
//         _documentUrlService = documentUrlService;
//         _contentService = contentService;
//         _idKeyMap = idKeyMap;
//     }
//
//     // TODO move to content refreshers
//     public async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken) => await _documentUrlService.CreateOrUpdateUrlSegmentsAsync(notification.PublishedEntities);
//     public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken) => await _documentUrlService.DeleteUrlsAsync(notification.DeletedEntities);
//     public async Task HandleAsync(ContentUnpublishedNotification notification, CancellationToken cancellationToken) => await _documentUrlService.DeleteUrlsAsync(notification.UnpublishedEntities);
//
//     public async Task HandleAsync(ContentSavedNotification notification, CancellationToken cancellationToken) => await _documentUrlService.CreateOrUpdateUrlSegmentsAsync(notification.SavedEntities);
// }
