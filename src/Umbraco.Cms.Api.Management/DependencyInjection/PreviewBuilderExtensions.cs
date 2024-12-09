using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Preview;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class PreviewBuilderExtensions
{
    internal static IUmbracoBuilder AddPreview(this IUmbracoBuilder builder)
    {
        builder.Services.AddSignalR().AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        builder.Services.AddSingleton<IServerEventRouter, ServerEventRouter>();
        builder.Services.AddSingleton<PreviewRoutes>();
        builder.AddNotificationAsyncHandler<ContentCacheRefresherNotification, PreviewHubUpdater>();
        builder.AddNotificationHandler<ContentSavedNotification, ServerEventSender>();

        return builder;
    }
}

