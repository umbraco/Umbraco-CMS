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
        AddEvents(builder);

        return builder;
    }

    private static void AddEvents(IUmbracoBuilder builder)
    {
        builder.AddNotificationAsyncHandler<ContentSavedNotification, ServerEventSender>();
    }
}
