using Umbraco.Cms.Api.Management.ServerEvents.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Api.Management.ServerEvents;

public class ServerEventSender :
    INotificationAsyncHandler<ContentSavedNotification>,
    INotificationAsyncHandler<ContentTypeSavedNotification>,
    INotificationAsyncHandler<MediaSavedNotification>,
    INotificationAsyncHandler<MediaTypeSavedNotification>,
    INotificationAsyncHandler<MemberSavedNotification>,
    INotificationAsyncHandler<MemberTypeSavedNotification>,
    INotificationAsyncHandler<MemberGroupSavedNotification>,
    INotificationAsyncHandler<DataTypeSavedNotification>,
    INotificationAsyncHandler<LanguageSavedNotification>,
    INotificationAsyncHandler<ScriptSavedNotification>,
    INotificationAsyncHandler<StylesheetSavedNotification>,
    INotificationAsyncHandler<TemplateSavedNotification>,
    INotificationAsyncHandler<DictionaryItemSavedNotification>,
    INotificationAsyncHandler<DomainSavedNotification>,
    INotificationAsyncHandler<PartialViewSavedNotification>,
    INotificationAsyncHandler<PublicAccessEntrySavedNotification>,
    INotificationAsyncHandler<RelationSavedNotification>,
    INotificationAsyncHandler<RelationTypeSavedNotification>,
    INotificationAsyncHandler<UserGroupSavedNotification>,
    INotificationAsyncHandler<UserSavedNotification>,
    INotificationAsyncHandler<WebhookSavedNotification>,
    INotificationAsyncHandler<ContentDeletedNotification>,
    INotificationAsyncHandler<ContentTypeDeletedNotification>,
    INotificationAsyncHandler<MediaDeletedNotification>,
    INotificationAsyncHandler<MediaTypeDeletedNotification>,
    INotificationAsyncHandler<MemberDeletedNotification>,
    INotificationAsyncHandler<MemberTypeDeletedNotification>,
    INotificationAsyncHandler<MemberGroupDeletedNotification>,
    INotificationAsyncHandler<DataTypeDeletedNotification>,
    INotificationAsyncHandler<LanguageDeletedNotification>,
    INotificationAsyncHandler<ScriptDeletedNotification>,
    INotificationAsyncHandler<StylesheetDeletedNotification>,
    INotificationAsyncHandler<TemplateDeletedNotification>,
    INotificationAsyncHandler<DictionaryItemDeletedNotification>,
    INotificationAsyncHandler<DomainDeletedNotification>,
    INotificationAsyncHandler<PartialViewDeletedNotification>,
    INotificationAsyncHandler<PublicAccessEntryDeletedNotification>,
    INotificationAsyncHandler<RelationDeletedNotification>,
    INotificationAsyncHandler<RelationTypeDeletedNotification>,
    INotificationAsyncHandler<UserGroupDeletedNotification>,
    INotificationAsyncHandler<UserDeletedNotification>,
    INotificationAsyncHandler<WebhookDeletedNotification>
{
    private readonly IServerEventRouter _serverEventRouter;

    public ServerEventSender(IServerEventRouter serverEventRouter) => _serverEventRouter = serverEventRouter;

    private async Task NotifySavedAsync<T>(SavedNotification<T> notification, EventSource source)
        where T : IEntity
    {
        foreach (T entity in notification.SavedEntities)
        {
            EventType eventType = EventType.Updated;
            if (entity.CreateDate == entity.UpdateDate)
            {
                // This is a new entity
                eventType = EventType.Created;
            }

            var eventModel = new ServerEvent
            {
                EventType = eventType,
                Key = entity.Key,
                EventSource = source,
            };

            await _serverEventRouter.RouteEventAsync(eventModel);
        }
    }

    private async Task NotifyDeletedAsync<T>(DeletedNotification<T> notification, EventSource source)
        where T : IEntity
    {
        foreach (T entity in notification.DeletedEntities)
        {
            await _serverEventRouter.RouteEventAsync(new ServerEvent
            {
                EventType = EventType.Deleted, EventSource = source, Key = entity.Key,
            });
        }
    }

    public async Task HandleAsync(ContentSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.Document);

    public async Task HandleAsync(ContentTypeSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.DocumentType);

    public async Task HandleAsync(MediaSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.Media);

    public async Task HandleAsync(MediaTypeSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.MediaType);

    public async Task HandleAsync(MemberSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.Member);

    public async Task HandleAsync(MemberTypeSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.MemberType);

    public async Task HandleAsync(MemberGroupSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.MemberGroup);

    public async Task HandleAsync(DataTypeSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.DataType);

    public async Task HandleAsync(LanguageSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.Language);

    public async Task HandleAsync(ScriptSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.Script);

    public async Task HandleAsync(StylesheetSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.Stylesheet);

    public async Task HandleAsync(TemplateSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.Template);

    public async Task HandleAsync(DictionaryItemSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.DictionaryItem);

    public async Task HandleAsync(DomainSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.Domain);

    public async Task HandleAsync(PartialViewSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.PartialView);

    public async Task HandleAsync(PublicAccessEntrySavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.PublicAccessEntry);

    public async Task HandleAsync(RelationSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.Relation);

    public async Task HandleAsync(RelationTypeSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.RelationType);

    public async Task HandleAsync(UserGroupSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.UserGroup);

    public async Task HandleAsync(UserSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.User);

    public async Task HandleAsync(WebhookSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, EventSource.Webhook);

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.Document);

    public async Task HandleAsync(ContentTypeDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.DocumentType);

    public async Task HandleAsync(MediaDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.Media);

    public async Task HandleAsync(MediaTypeDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.MediaType);

    public async Task HandleAsync(MemberDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.Member);

    public async Task HandleAsync(MemberTypeDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.MemberType);

    public async Task HandleAsync(MemberGroupDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.MemberGroup);

    public async Task HandleAsync(DataTypeDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.DataType);

    public async Task HandleAsync(LanguageDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.Language);

    public async Task HandleAsync(ScriptDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.Script);

    public async Task HandleAsync(StylesheetDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.Stylesheet);

    public async Task HandleAsync(TemplateDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.Template);

    public async Task HandleAsync(DictionaryItemDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.DictionaryItem);

    public async Task HandleAsync(DomainDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.Domain);

    public async Task HandleAsync(PartialViewDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.PartialView);

    public async Task HandleAsync(PublicAccessEntryDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.PublicAccessEntry);

    public async Task HandleAsync(RelationDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.Relation);

    public async Task HandleAsync(RelationTypeDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.RelationType);

    public async Task HandleAsync(UserGroupDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.UserGroup);

    public async Task HandleAsync(UserDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.User);

    public async Task HandleAsync(WebhookDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, EventSource.Webhook);
}
