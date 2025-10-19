using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.ServerEvents;

namespace Umbraco.Cms.Api.Management.ServerEvents;

internal sealed class ServerEventSender :
    INotificationAsyncHandler<ContentSavedNotification>,
    INotificationAsyncHandler<ContentSavedBlueprintNotification>,
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
    INotificationAsyncHandler<ContentDeletedBlueprintNotification>,
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
    INotificationAsyncHandler<WebhookDeletedNotification>,
    INotificationAsyncHandler<ContentMovedToRecycleBinNotification>,
    INotificationAsyncHandler<MediaMovedToRecycleBinNotification>
{
    private readonly IServerEventRouter _serverEventRouter;

    public ServerEventSender(IServerEventRouter serverEventRouter) => _serverEventRouter = serverEventRouter;

    private async Task NotifySavedAsync<T>(SavedNotification<T> notification, string source)
        where T : IEntity
    {
        foreach (T entity in notification.SavedEntities)
        {
            await RouteCreatedOrUpdatedEvent(source, entity);
        }
    }

    private async Task RouteCreatedOrUpdatedEvent<T>(string source, T entity) where T : IEntity
    {
        var eventModel = new ServerEvent
        {
            EventType = entity.CreateDate == entity.UpdateDate
                ? Constants.ServerEvents.EventType.Created : Constants.ServerEvents.EventType.Updated,
            Key = entity.Key,
            EventSource = source,
        };

        await _serverEventRouter.RouteEventAsync(eventModel);
    }

    private async Task NotifyDeletedAsync<T>(DeletedNotification<T> notification, string source)
        where T : IEntity
    {
        foreach (T entity in notification.DeletedEntities)
        {
            await RouteDeletedEvent(source, entity);
        }
    }

    private async Task RouteDeletedEvent<T>(string source, T entity) where T : IEntity =>
        await _serverEventRouter.RouteEventAsync(new ServerEvent
        {
            EventType = Constants.ServerEvents.EventType.Deleted,
            EventSource = source,
            Key = entity.Key,
        });

    private async Task NotifyTrashedAsync<T>(MovedToRecycleBinNotification<T> notification, string source)
        where T : IEntity
    {
        foreach (MoveToRecycleBinEventInfo<T> movedEvent in notification.MoveInfoCollection)
        {
            await _serverEventRouter.RouteEventAsync(new ServerEvent
            {
                EventType = Constants.ServerEvents.EventType.Trashed,
                EventSource = source,
                Key = movedEvent.Entity.Key,
            });
        }
    }

    public async Task HandleAsync(ContentSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Document);

    public async Task HandleAsync(ContentSavedBlueprintNotification notification, CancellationToken cancellationToken) =>
        await RouteCreatedOrUpdatedEvent(Constants.ServerEvents.EventSource.DocumentBlueprint, notification.SavedBlueprint);

    public async Task HandleAsync(ContentTypeSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.DocumentType);

    public async Task HandleAsync(MediaSavedNotification notification, CancellationToken cancellationToken)
        => await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Media);

    public async Task HandleAsync(MediaTypeSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.MediaType);

    public async Task HandleAsync(MemberSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Member);

    public async Task HandleAsync(MemberTypeSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.MemberType);

    public async Task HandleAsync(MemberGroupSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.MemberGroup);

    public async Task HandleAsync(DataTypeSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.DataType);

    public async Task HandleAsync(LanguageSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Language);

    public async Task HandleAsync(ScriptSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Script);

    public async Task HandleAsync(StylesheetSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Stylesheet);

    public async Task HandleAsync(TemplateSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Template);

    public async Task HandleAsync(DictionaryItemSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.DictionaryItem);

    public async Task HandleAsync(DomainSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Domain);

    public async Task HandleAsync(PartialViewSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.PartialView);

    public async Task HandleAsync(PublicAccessEntrySavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.PublicAccessEntry);

    public async Task HandleAsync(RelationSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Relation);

    public async Task HandleAsync(RelationTypeSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.RelationType);

    public async Task HandleAsync(UserGroupSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.UserGroup);

    public async Task HandleAsync(UserSavedNotification notification, CancellationToken cancellationToken)
    {
        // We still need to notify of saved entities like any other event source.
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.User);

        // But for users we also want to notify each updated user that they have been updated separately.
        foreach (IUser user in notification.SavedEntities)
        {
            var eventModel = new ServerEvent
            {
                EventType = Constants.ServerEvents.EventType.Updated,
                Key = user.Key,
                EventSource = Constants.ServerEvents.EventSource.CurrentUser,
            };

            await _serverEventRouter.NotifyUserAsync(eventModel, user.Key);
        }
    }

    public async Task HandleAsync(WebhookSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Webhook);

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Document);

    public async Task HandleAsync(ContentDeletedBlueprintNotification notification, CancellationToken cancellationToken)
    {
        foreach (Core.Models.IContent entity in notification.DeletedBlueprints)
        {
            await RouteDeletedEvent(Constants.ServerEvents.EventSource.DocumentBlueprint, entity);
        }
    }

    public async Task HandleAsync(ContentTypeDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.DocumentType);

    public async Task HandleAsync(MediaDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Media);

    public async Task HandleAsync(MediaTypeDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.MediaType);

    public async Task HandleAsync(MemberDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Member);

    public async Task HandleAsync(MemberTypeDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.MemberType);

    public async Task HandleAsync(MemberGroupDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.MemberGroup);

    public async Task HandleAsync(DataTypeDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.DataType);

    public async Task HandleAsync(LanguageDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Language);

    public async Task HandleAsync(ScriptDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Script);

    public async Task HandleAsync(StylesheetDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Stylesheet);

    public async Task HandleAsync(TemplateDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Template);

    public async Task HandleAsync(DictionaryItemDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.DictionaryItem);

    public async Task HandleAsync(DomainDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Domain);

    public async Task HandleAsync(PartialViewDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.PartialView);

    public async Task HandleAsync(PublicAccessEntryDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.PublicAccessEntry);

    public async Task HandleAsync(RelationDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Relation);

    public async Task HandleAsync(RelationTypeDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.RelationType);

    public async Task HandleAsync(UserGroupDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.UserGroup);

    public async Task HandleAsync(UserDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.User);

    public async Task HandleAsync(WebhookDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Webhook);

    public async Task HandleAsync(ContentMovedToRecycleBinNotification notification, CancellationToken cancellationToken) =>
        await NotifyTrashedAsync(notification, Constants.ServerEvents.EventSource.Document);

    public async Task HandleAsync(MediaMovedToRecycleBinNotification notification, CancellationToken cancellationToken) =>
        await NotifyTrashedAsync(notification, Constants.ServerEvents.EventSource.Media);
}
