using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ServerEvents;
using Umbraco.Cms.Api.Management.ServerEvents.Authorizers;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.ServerEvents;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class ServerEventExtensions
{
    internal static IUmbracoBuilder AddServerEvents(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IUserConnectionManager, UserConnectionManager>();
        builder.Services.AddSingleton<IServerEventRouter, ServerEventRouter>();
        builder.Services.AddSingleton<IServerEventUserManager, ServerEventUserManager>();
        builder.Services.AddSingleton<IServerEventAuthorizationService, ServerEventAuthorizationService>();
        builder.AddNotificationAsyncHandler<UserSavedNotification, UserConnectionRefresher>();

        builder
            .AddEvents()
            .AddAuthorizers();

        return builder;
    }

    private static IUmbracoBuilder AddEvents(this IUmbracoBuilder builder)
    {
        builder.AddNotificationAsyncHandler<ContentSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<ContentSavedBlueprintNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<ContentTypeSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<MediaSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<MediaTypeSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<MemberSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<MemberTypeSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<MemberGroupSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<DataTypeSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<LanguageSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<ScriptSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<StylesheetSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<TemplateSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<DictionaryItemSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<DomainSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<PartialViewSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<PublicAccessEntrySavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<RelationSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<RelationTypeSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<UserGroupSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<UserSavedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<WebhookSavedNotification, ServerEventSender>();

        builder.AddNotificationAsyncHandler<ContentDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<ContentDeletedBlueprintNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<ContentTypeDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<MediaDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<MediaTypeDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<MemberDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<MemberTypeDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<MemberGroupDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<DataTypeDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<LanguageDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<ScriptDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<StylesheetDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<TemplateDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<DictionaryItemDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<DomainDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<PartialViewDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<PublicAccessEntryDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<RelationDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<RelationTypeDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<UserGroupDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<UserDeletedNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<WebhookDeletedNotification, ServerEventSender>();

        builder.AddNotificationAsyncHandler<ContentMovedToRecycleBinNotification, ServerEventSender>();
        builder.AddNotificationAsyncHandler<MediaMovedToRecycleBinNotification, ServerEventSender>();

        return builder;
    }

    private static IUmbracoBuilder AddAuthorizers(this IUmbracoBuilder builder)
    {
        builder.EventSourceAuthorizers()
            .Append<DocumentEventAuthorizer>()
            .Append<DocumentBlueprintEventAuthorizer>()
            .Append<DocumentTypeEventAuthorizer>()
            .Append<MediaEventAuthorizer>()
            .Append<MediaTypeEventAuthorizer>()
            .Append<MemberEventAuthorizer>()
            .Append<MemberGroupEventAuthorizer>()
            .Append<MemberTypeEventAuthorizer>()
            .Append<DataTypeEventAuthorizer>()
            .Append<LanguageEventAuthorizer>()
            .Append<ScriptEventAuthorizer>()
            .Append<StylesheetEventAuthorizer>()
            .Append<TemplateEventAuthorizer>()
            .Append<DictionaryItemEventAuthorizer>()
            .Append<DomainEventAuthorizer>()
            .Append<PartialViewEventAuthorizer>()
            .Append<PublicAccessEntryEventAuthorizer>()
            .Append<RelationEventAuthorizer>()
            .Append<RelationTypeEventAuthorizer>()
            .Append<UserGroupEventAuthorizer>()
            .Append<UserEventAuthorizer>()
            .Append<WebhookEventAuthorizer>();
        return builder;
    }
}
