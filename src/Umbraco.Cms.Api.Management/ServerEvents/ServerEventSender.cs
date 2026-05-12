using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <summary>
/// Handles server event notifications for various entity types by routing create, update, delete, and recycle bin
/// events to connected clients or systems.
/// </summary>
internal sealed class ServerEventSender :
    IDistributedCacheNotificationHandler,
    INotificationAsyncHandler<ContentDeletedBlueprintNotification>,
    INotificationAsyncHandler<ContentDeletedNotification>,
    INotificationAsyncHandler<ContentMovedToRecycleBinNotification>,
    INotificationAsyncHandler<ContentSavedBlueprintNotification>,
    INotificationAsyncHandler<ContentSavedNotification>,
    INotificationAsyncHandler<ContentTypeChangedNotification>,
    INotificationAsyncHandler<ContentTypeDeletedNotification>,
    INotificationAsyncHandler<ContentTypeSavedNotification>,
    INotificationAsyncHandler<DataTypeDeletedNotification>,
    INotificationAsyncHandler<DataTypeSavedNotification>,
    INotificationAsyncHandler<DictionaryItemDeletedNotification>,
    INotificationAsyncHandler<DictionaryItemSavedNotification>,
    INotificationAsyncHandler<DomainDeletedNotification>,
    INotificationAsyncHandler<DomainSavedNotification>,
    INotificationAsyncHandler<LanguageDeletedNotification>,
    INotificationAsyncHandler<LanguageSavedNotification>,
    INotificationAsyncHandler<MediaDeletedNotification>,
    INotificationAsyncHandler<MediaMovedToRecycleBinNotification>,
    INotificationAsyncHandler<MediaSavedNotification>,
    INotificationAsyncHandler<MediaTypeChangedNotification>,
    INotificationAsyncHandler<MediaTypeDeletedNotification>,
    INotificationAsyncHandler<MediaTypeSavedNotification>,
    INotificationAsyncHandler<MemberDeletedNotification>,
    INotificationAsyncHandler<MemberGroupDeletedNotification>,
    INotificationAsyncHandler<MemberGroupSavedNotification>,
    INotificationAsyncHandler<MemberSavedNotification>,
    INotificationAsyncHandler<MemberTypeChangedNotification>,
    INotificationAsyncHandler<MemberTypeDeletedNotification>,
    INotificationAsyncHandler<MemberTypeSavedNotification>,
    INotificationAsyncHandler<PartialViewDeletedNotification>,
    INotificationAsyncHandler<PartialViewSavedNotification>,
    INotificationAsyncHandler<PublicAccessEntryDeletedNotification>,
    INotificationAsyncHandler<PublicAccessEntrySavedNotification>,
    INotificationAsyncHandler<RelationDeletedNotification>,
    INotificationAsyncHandler<RelationSavedNotification>,
    INotificationAsyncHandler<RelationTypeDeletedNotification>,
    INotificationAsyncHandler<RelationTypeSavedNotification>,
    INotificationAsyncHandler<ScriptDeletedNotification>,
    INotificationAsyncHandler<ScriptSavedNotification>,
    INotificationAsyncHandler<StylesheetDeletedNotification>,
    INotificationAsyncHandler<StylesheetSavedNotification>,
    INotificationAsyncHandler<TemplateDeletedNotification>,
    INotificationAsyncHandler<TemplateSavedNotification>,
    INotificationAsyncHandler<UserDeletedNotification>,
    INotificationAsyncHandler<UserGroupDeletedNotification>,
    INotificationAsyncHandler<UserGroupSavedNotification>,
    INotificationAsyncHandler<UserSavedNotification>,
    INotificationAsyncHandler<WebhookDeletedNotification>,
    INotificationAsyncHandler<WebhookSavedNotification>
{
    private readonly IServerEventRouter _serverEventRouter;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerEventSender"/> class.
    /// </summary>
    /// <param name="serverEventRouter">The router responsible for handling server events.</param>
    /// <param name="idKeyMap">The map used to resolve identifier keys.</param>
    public ServerEventSender(IServerEventRouter serverEventRouter, IIdKeyMap idKeyMap)
    {
        _serverEventRouter = serverEventRouter;
        _idKeyMap = idKeyMap;
    }

    /// <inheritdoc/>
    public Task HandleAsync(ContentDeletedBlueprintNotification notification, CancellationToken cancellationToken)
        => NotifyBlueprintDeletedAsync(notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<ContentDeletedBlueprintNotification> notifications, CancellationToken cancellationToken)
        => NotifyBlueprintDeletedAsync(notifications);

    /// <inheritdoc/>
    public Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Document, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<ContentDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Document, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(ContentMovedToRecycleBinNotification notification, CancellationToken cancellationToken)
        => NotifyTrashedAsync(Constants.ServerEvents.EventSource.Document, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<ContentMovedToRecycleBinNotification> notifications, CancellationToken cancellationToken)
        => NotifyTrashedAsync(Constants.ServerEvents.EventSource.Document, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(ContentSavedBlueprintNotification notification, CancellationToken cancellationToken)
        => NotifyBlueprintSavedAsync(notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<ContentSavedBlueprintNotification> notifications, CancellationToken cancellationToken)
        => NotifyBlueprintSavedAsync(notifications);

    /// <inheritdoc/>
    public Task HandleAsync(ContentSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Document, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<ContentSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Document, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(ContentTypeChangedNotification notification, CancellationToken cancellationToken)
        => RouteContentTypeChangedEventsAsync(Constants.ServerEvents.EventSource.DocumentType, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<ContentTypeChangedNotification> notifications, CancellationToken cancellationToken)
        => RouteContentTypeChangedEventsAsync(Constants.ServerEvents.EventSource.DocumentType, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(ContentTypeDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.DocumentType, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<ContentTypeDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.DocumentType, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(ContentTypeSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.DocumentType, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<ContentTypeSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.DocumentType, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(DataTypeDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.DataType, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<DataTypeDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.DataType, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(DataTypeSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.DataType, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<DataTypeSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.DataType, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(DictionaryItemDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.DictionaryItem, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<DictionaryItemDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.DictionaryItem, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(DictionaryItemSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.DictionaryItem, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<DictionaryItemSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.DictionaryItem, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(DomainDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Domain, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<DomainDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Domain, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(DomainSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Domain, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<DomainSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Domain, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(LanguageDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Language, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<LanguageDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Language, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(LanguageSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Language, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<LanguageSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Language, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(MediaDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Media, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<MediaDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Media, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(MediaMovedToRecycleBinNotification notification, CancellationToken cancellationToken)
        => NotifyTrashedAsync(Constants.ServerEvents.EventSource.Media, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<MediaMovedToRecycleBinNotification> notifications, CancellationToken cancellationToken)
        => NotifyTrashedAsync(Constants.ServerEvents.EventSource.Media, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(MediaSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Media, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<MediaSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Media, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(MediaTypeChangedNotification notification, CancellationToken cancellationToken)
        => RouteContentTypeChangedEventsAsync(Constants.ServerEvents.EventSource.MediaType, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<MediaTypeChangedNotification> notifications, CancellationToken cancellationToken)
        => RouteContentTypeChangedEventsAsync(Constants.ServerEvents.EventSource.MediaType, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(MediaTypeDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.MediaType, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<MediaTypeDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.MediaType, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(MediaTypeSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.MediaType, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<MediaTypeSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.MediaType, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(MemberDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Member, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<MemberDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Member, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(MemberGroupDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.MemberGroup, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<MemberGroupDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.MemberGroup, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(MemberGroupSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.MemberGroup, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<MemberGroupSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.MemberGroup, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(MemberSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Member, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<MemberSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Member, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(MemberTypeChangedNotification notification, CancellationToken cancellationToken)
        => RouteContentTypeChangedEventsAsync(Constants.ServerEvents.EventSource.MemberType, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<MemberTypeChangedNotification> notifications, CancellationToken cancellationToken)
        => RouteContentTypeChangedEventsAsync(Constants.ServerEvents.EventSource.MemberType, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(MemberTypeDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.MemberType, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<MemberTypeDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.MemberType, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(MemberTypeSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.MemberType, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<MemberTypeSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.MemberType, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(PartialViewDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.PartialView, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<PartialViewDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.PartialView, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(PartialViewSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.PartialView, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<PartialViewSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.PartialView, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(PublicAccessEntryDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyPublicAccessEntryDeletedAsync(notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<PublicAccessEntryDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyPublicAccessEntryDeletedAsync(notifications);

    /// <inheritdoc/>
    public Task HandleAsync(PublicAccessEntrySavedNotification notification, CancellationToken cancellationToken)
        => NotifyPublicAccessEntrySavedAsync(notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<PublicAccessEntrySavedNotification> notifications, CancellationToken cancellationToken)
        => NotifyPublicAccessEntrySavedAsync(notifications);

    /// <inheritdoc/>
    public Task HandleAsync(RelationDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Relation, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<RelationDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Relation, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(RelationSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Relation, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<RelationSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Relation, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(RelationTypeDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.RelationType, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<RelationTypeDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.RelationType, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(RelationTypeSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.RelationType, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<RelationTypeSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.RelationType, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(ScriptDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Script, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<ScriptDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Script, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(ScriptSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Script, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<ScriptSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Script, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(StylesheetDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Stylesheet, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<StylesheetDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Stylesheet, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(StylesheetSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Stylesheet, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<StylesheetSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Stylesheet, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(TemplateDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Template, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<TemplateDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Template, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(TemplateSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Template, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<TemplateSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Template, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(UserDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.User, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<UserDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.User, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(UserGroupDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.UserGroup, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<UserGroupDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.UserGroup, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(UserGroupSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.UserGroup, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<UserGroupSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.UserGroup, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(UserSavedNotification notification, CancellationToken cancellationToken)
        => NotifyUserSavedAsync(notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<UserSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifyUserSavedAsync(notifications);

    /// <inheritdoc/>
    public Task HandleAsync(WebhookDeletedNotification notification, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Webhook, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<WebhookDeletedNotification> notifications, CancellationToken cancellationToken)
        => NotifyDeletedAsync(Constants.ServerEvents.EventSource.Webhook, notifications);

    /// <inheritdoc/>
    public Task HandleAsync(WebhookSavedNotification notification, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Webhook, notification);

    /// <inheritdoc/>
    public Task HandleAsync(IEnumerable<WebhookSavedNotification> notifications, CancellationToken cancellationToken)
        => NotifySavedAsync(Constants.ServerEvents.EventSource.Webhook, notifications);

    private async Task NotifyBlueprintDeletedAsync(params IEnumerable<ContentDeletedBlueprintNotification> notifications)
    {
        var seen = new HashSet<Guid>();
        foreach (ContentDeletedBlueprintNotification notification in notifications)
        {
            foreach (IContent entity in notification.DeletedBlueprints)
            {
                if (seen.Add(entity.Key))
                {
                    await RouteDeletedEvent(Constants.ServerEvents.EventSource.DocumentBlueprint, entity);
                }
            }
        }
    }

    private async Task NotifyDeletedAsync<T>(string source, params IEnumerable<DeletedNotification<T>> notifications)
        where T : IEntity
    {
        var seen = new HashSet<Guid>();
        foreach (DeletedNotification<T> notification in notifications)
        {
            foreach (T entity in notification.DeletedEntities)
            {
                if (seen.Add(entity.Key))
                {
                    await RouteDeletedEvent(source, entity);
                }
            }
        }
    }

    private async Task NotifyTrashedAsync<T>(string source, params IEnumerable<MovedToRecycleBinNotification<T>> notifications)
        where T : IEntity
    {
        var seen = new HashSet<Guid>();
        foreach (MovedToRecycleBinNotification<T> notification in notifications)
        {
            foreach (MoveToRecycleBinEventInfo<T> movedEvent in notification.MoveInfoCollection)
            {
                if (seen.Add(movedEvent.Entity.Key))
                {
                    await _serverEventRouter.RouteEventAsync(new ServerEvent
                    {
                        EventType = Constants.ServerEvents.EventType.Trashed,
                        EventSource = source,
                        Key = movedEvent.Entity.Key,
                    });
                }
            }
        }
    }

    private async Task NotifyBlueprintSavedAsync(params IEnumerable<ContentSavedBlueprintNotification> notifications)
    {
        var seen = new HashSet<Guid>();
        foreach (ContentSavedBlueprintNotification notification in notifications)
        {
            if (seen.Add(notification.SavedBlueprint.Key))
            {
                await RouteCreatedOrUpdatedEvent(Constants.ServerEvents.EventSource.DocumentBlueprint, notification.SavedBlueprint);
            }
        }
    }

    private async Task NotifySavedAsync<T>(string source, params IEnumerable<SavedNotification<T>> notifications)
        where T : IEntity
    {
        var seen = new HashSet<Guid>();
        foreach (SavedNotification<T> notification in notifications)
        {
            foreach (T entity in notification.SavedEntities)
            {
                if (seen.Add(entity.Key))
                {
                    await RouteCreatedOrUpdatedEvent(source, entity);
                }
            }
        }
    }

    private async Task RouteContentTypeChangedEventsAsync<T>(string source, params IEnumerable<ContentTypeChangeNotification<T>> notifications)
        where T : class, IContentTypeComposition
    {
        var seen = new HashSet<Guid>();
        foreach (ContentTypeChangeNotification<T> notification in notifications)
        {
            foreach (ContentTypeChange<T> change in notification.Changes)
            {
                // Skip created and removed types as these are already handled by the Deleted
                // and Saved notification handler respectively.
                if (change.ChangeTypes.HasType(ContentTypeChangeTypes.Create) ||
                    change.ChangeTypes.HasType(ContentTypeChangeTypes.Remove))
                {
                    continue;
                }

                if (seen.Add(change.Item.Key) is false)
                {
                    continue;
                }

                await _serverEventRouter.RouteEventAsync(new ServerEvent
                {
                    EventType = Constants.ServerEvents.EventType.Updated,
                    Key = change.Item.Key,
                    EventSource = source,
                });
            }
        }
    }

    private async Task NotifyPublicAccessEntryDeletedAsync(params IEnumerable<PublicAccessEntryDeletedNotification> notifications)
    {
        // Materialize so we can iterate twice (once for the entries, once for the affected documents).
        IReadOnlyCollection<PublicAccessEntryDeletedNotification> materialized =
            notifications as IReadOnlyCollection<PublicAccessEntryDeletedNotification> ?? notifications.ToList();

        await NotifyDeletedAsync(Constants.ServerEvents.EventSource.PublicAccessEntry, materialized);

        // For public access entries, we also need to notify affected content items, so any client-side
        // cache for the document can be invalidated.
        await RouteDocumentUpdatedEventForPublicAccessModification(materialized.SelectMany(x => x.DeletedEntities));
    }

    private async Task NotifyPublicAccessEntrySavedAsync(params IEnumerable<PublicAccessEntrySavedNotification> notifications)
    {
        // Materialize so we can iterate twice (once for the entries, once for the affected documents).
        IReadOnlyCollection<PublicAccessEntrySavedNotification> materialized = notifications as IReadOnlyCollection<PublicAccessEntrySavedNotification> ?? notifications.ToList();

        await NotifySavedAsync(Constants.ServerEvents.EventSource.PublicAccessEntry, materialized);

        // For public access entries, we also need to notify affected content items, so any client-side
        // cache for the document can be invalidated.
        await RouteDocumentUpdatedEventForPublicAccessModification(materialized.SelectMany(x => x.SavedEntities));
    }

    private async Task NotifyUserSavedAsync(params IEnumerable<UserSavedNotification> notifications)
    {
        // Materialize so we can iterate twice.
        IReadOnlyCollection<UserSavedNotification> materialized = notifications as IReadOnlyCollection<UserSavedNotification> ?? notifications.ToList();

        // We still need to notify of saved entities like any other event source.
        await NotifySavedAsync(Constants.ServerEvents.EventSource.User, materialized);

        // But for users we also want to notify each updated user that they have been updated separately.
        var seen = new HashSet<Guid>();
        foreach (IUser user in materialized.SelectMany(x => x.SavedEntities))
        {
            if (seen.Add(user.Key) is false)
            {
                continue;
            }

            await _serverEventRouter.NotifyUserAsync(
                new ServerEvent
                {
                    EventType = Constants.ServerEvents.EventType.Updated,
                    Key = user.Key,
                    EventSource = Constants.ServerEvents.EventSource.CurrentUser,
                },
                user.Key);
        }
    }

    private async Task RouteDeletedEvent<T>(string source, T entity)
        where T : IEntity
        => await _serverEventRouter.RouteEventAsync(new ServerEvent
        {
            EventType = Constants.ServerEvents.EventType.Deleted,
            EventSource = source,
            Key = entity.Key,
        });

    private async Task RouteCreatedOrUpdatedEvent<T>(string source, T entity)
        where T : IEntity
        => await _serverEventRouter.RouteEventAsync(new ServerEvent
        {
            EventType = entity.CreateDate == entity.UpdateDate
                ? Constants.ServerEvents.EventType.Created
                : Constants.ServerEvents.EventType.Updated,
            Key = entity.Key,
            EventSource = source,
        });

    private async Task RouteDocumentUpdatedEventForPublicAccessModification(IEnumerable<PublicAccessEntry> entities)
    {
        var seen = new HashSet<Guid>();
        foreach (PublicAccessEntry entity in entities)
        {
            Attempt<Guid> getKeyAttempt = _idKeyMap.GetKeyForId(entity.ProtectedNodeId, UmbracoObjectTypes.Document);
            if (getKeyAttempt.Success is false)
            {
                continue;
            }

            if (seen.Add(getKeyAttempt.Result) is false)
            {
                continue;
            }

            await _serverEventRouter.RouteEventAsync(new ServerEvent
            {
                EventType = Constants.ServerEvents.EventType.Updated,
                Key = getKeyAttempt.Result,
                EventSource = Constants.ServerEvents.EventSource.Document,
            });
        }
    }
}
