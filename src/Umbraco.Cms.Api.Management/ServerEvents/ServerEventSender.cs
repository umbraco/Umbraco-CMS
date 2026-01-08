using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <summary>
/// Handles server event notifications for various entity types by routing create, update, delete, and recycle bin
/// events to connected clients or systems.
/// </summary>
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
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerEventSender"/> class.
    /// </summary>
    public ServerEventSender(IServerEventRouter serverEventRouter, IIdKeyMap idKeyMap)
    {
        _serverEventRouter = serverEventRouter;
        _idKeyMap = idKeyMap;
    }

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

    /// <inheritdoc/>
    public async Task HandleAsync(ContentSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Document);

    /// <inheritdoc/>
    public async Task HandleAsync(ContentSavedBlueprintNotification notification, CancellationToken cancellationToken) =>
        await RouteCreatedOrUpdatedEvent(Constants.ServerEvents.EventSource.DocumentBlueprint, notification.SavedBlueprint);

    /// <inheritdoc/>
    public async Task HandleAsync(ContentTypeSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.DocumentType);

    /// <inheritdoc/>
    public async Task HandleAsync(MediaSavedNotification notification, CancellationToken cancellationToken)
        => await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Media);

    /// <inheritdoc/>
    public async Task HandleAsync(MediaTypeSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.MediaType);

    /// <inheritdoc/>
    public async Task HandleAsync(MemberSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Member);

    /// <inheritdoc/>
    public async Task HandleAsync(MemberTypeSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.MemberType);

    /// <inheritdoc/>
    public async Task HandleAsync(MemberGroupSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.MemberGroup);

    /// <inheritdoc/>
    public async Task HandleAsync(DataTypeSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.DataType);

    /// <inheritdoc/>
    public async Task HandleAsync(LanguageSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Language);

    /// <inheritdoc/>
    public async Task HandleAsync(ScriptSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Script);

    /// <inheritdoc/>
    public async Task HandleAsync(StylesheetSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Stylesheet);

    /// <inheritdoc/>
    public async Task HandleAsync(TemplateSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Template);

    /// <inheritdoc/>
    public async Task HandleAsync(DictionaryItemSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.DictionaryItem);

    /// <inheritdoc/>
    public async Task HandleAsync(DomainSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Domain);

    /// <inheritdoc/>
    public async Task HandleAsync(PartialViewSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.PartialView);

    /// <inheritdoc/>
    public async Task HandleAsync(PublicAccessEntrySavedNotification notification, CancellationToken cancellationToken)
    {
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.PublicAccessEntry);

        // For public access entries, we also need to notify affected content items, so any client-side
        // cache for the document can be invalidated.
        await RouteDocumentUpdatedEventForPublicAccessModification(notification.SavedEntities);
    }

    private async Task RouteDocumentUpdatedEventForPublicAccessModification(IEnumerable<PublicAccessEntry> entities)
    {
        foreach (PublicAccessEntry entity in entities)
        {
            Attempt<Guid> getKeyAttempt = _idKeyMap.GetKeyForId(
                entity.ProtectedNodeId,
                UmbracoObjectTypes.Document);
            if (getKeyAttempt.Success is false)
            {
                continue;
            }

            var eventModel = new ServerEvent
            {
                EventType = Constants.ServerEvents.EventType.Updated,
                Key = getKeyAttempt.Result,
                EventSource = Constants.ServerEvents.EventSource.Document,
            };
            await _serverEventRouter.RouteEventAsync(eventModel);
        }
    }

    /// <inheritdoc/>
    public async Task HandleAsync(RelationSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Relation);

    /// <inheritdoc/>
    public async Task HandleAsync(RelationTypeSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.RelationType);

    /// <inheritdoc/>
    public async Task HandleAsync(UserGroupSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.UserGroup);

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task HandleAsync(WebhookSavedNotification notification, CancellationToken cancellationToken) =>
        await NotifySavedAsync(notification, Constants.ServerEvents.EventSource.Webhook);

    /// <inheritdoc/>
    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Document);

    /// <inheritdoc/>
    public async Task HandleAsync(ContentDeletedBlueprintNotification notification, CancellationToken cancellationToken)
    {
        foreach (Core.Models.IContent entity in notification.DeletedBlueprints)
        {
            await RouteDeletedEvent(Constants.ServerEvents.EventSource.DocumentBlueprint, entity);
        }
    }

    /// <inheritdoc/>
    public async Task HandleAsync(ContentTypeDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.DocumentType);

    /// <inheritdoc/>
    public async Task HandleAsync(MediaDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Media);

    /// <inheritdoc/>
    public async Task HandleAsync(MediaTypeDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.MediaType);

    /// <inheritdoc/>
    public async Task HandleAsync(MemberDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Member);

    /// <inheritdoc/>
    public async Task HandleAsync(MemberTypeDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.MemberType);

    /// <inheritdoc/>
    public async Task HandleAsync(MemberGroupDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.MemberGroup);

    /// <inheritdoc/>
    public async Task HandleAsync(DataTypeDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.DataType);

    /// <inheritdoc/>
    public async Task HandleAsync(LanguageDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Language);

    /// <inheritdoc/>
    public async Task HandleAsync(ScriptDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Script);

    /// <inheritdoc/>
    public async Task HandleAsync(StylesheetDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Stylesheet);

    /// <inheritdoc/>
    public async Task HandleAsync(TemplateDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Template);

    /// <inheritdoc/>
    public async Task HandleAsync(DictionaryItemDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.DictionaryItem);

    /// <inheritdoc/>
    public async Task HandleAsync(DomainDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Domain);

    /// <inheritdoc/>
    public async Task HandleAsync(PartialViewDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.PartialView);

    /// <inheritdoc/>
    public async Task HandleAsync(PublicAccessEntryDeletedNotification notification, CancellationToken cancellationToken)
    {
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.PublicAccessEntry);

        // For public access entries, we also need to notify affected content items, so any client-side
        // cache for the document can be invalidated.
        await RouteDocumentUpdatedEventForPublicAccessModification(notification.DeletedEntities);
    }

    /// <inheritdoc/>
    public async Task HandleAsync(RelationDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Relation);

    /// <inheritdoc/>
    public async Task HandleAsync(RelationTypeDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.RelationType);

    /// <inheritdoc/>
    public async Task HandleAsync(UserGroupDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.UserGroup);

    /// <inheritdoc/>
    public async Task HandleAsync(UserDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.User);

    /// <inheritdoc/>
    public async Task HandleAsync(WebhookDeletedNotification notification, CancellationToken cancellationToken) =>
        await NotifyDeletedAsync(notification, Constants.ServerEvents.EventSource.Webhook);

    /// <inheritdoc/>
    public async Task HandleAsync(ContentMovedToRecycleBinNotification notification, CancellationToken cancellationToken) =>
        await NotifyTrashedAsync(notification, Constants.ServerEvents.EventSource.Document);

    /// <inheritdoc/>
    public async Task HandleAsync(MediaMovedToRecycleBinNotification notification, CancellationToken cancellationToken) =>
        await NotifyTrashedAsync(notification, Constants.ServerEvents.EventSource.Media);
}
