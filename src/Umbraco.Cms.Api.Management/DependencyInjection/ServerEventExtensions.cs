using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ServerEvents;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class ServerEventExtensions
{
    internal static IUmbracoBuilder AddServerEvents(this IUmbracoBuilder builder)
    {
        builder.Services.AddSignalR().AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        builder.Services.AddSingleton<IUserConnectionManager, UserConnectionManager>();
        builder.Services.AddSingleton<IServerEventRouter, ServerEventRouter>();
        builder.Services.AddSingleton<IServerEventUserManager, ServerEventUserManager>();
        builder.AddNotificationAsyncHandler<UserSavedNotification, UserConnectionRefresher>();

        AddEvents(builder);

        return builder;
    }

    private static void AddEvents(IUmbracoBuilder builder)
    {
        builder.AddNotificationAsyncHandler<ContentSavedNotification, ServerEventSender>();
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
    }
}
