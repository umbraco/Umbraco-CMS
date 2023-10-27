﻿using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Webhooks.Events;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Webhooks;

public class WebhookEventCollectionBuilder : OrderedCollectionBuilderBase<WebhookEventCollectionBuilder, WebhookEventCollection, IWebhookEvent>
{
    protected override WebhookEventCollectionBuilder This => this;

    public override void RegisterWith(IServiceCollection services)
    {
        // register the collection
        services.Add(new ServiceDescriptor(typeof(WebhookEventCollection), CreateCollection, ServiceLifetime.Singleton));

        // register the types
        RegisterTypes(services);
        base.RegisterWith(services);
    }

    public WebhookEventCollectionBuilder AddCoreWebhooks()
    {
        Append<ContentDeleteWebhookEvent>();
        Append<ContentPublishWebhookEvent>();
        Append<ContentUnpublishWebhookEvent>();
        Append<MediaDeleteWebhookEvent>();
        Append<MediaSaveWebhookEvent>();
        return this;
    }

    private void RegisterTypes(IServiceCollection services)
    {
        Type[] types = GetRegisteringTypes(GetTypes()).ToArray();

        // ensure they are safe
        foreach (Type type in types)
        {
            EnsureType(type, "register");
        }

        foreach (Type type in types)
        {
            Type? notificationType = GetNotificationType(type);

            if (notificationType is null)
            {
                continue;
            }

            var descriptor = new ServiceDescriptor(
                typeof(INotificationAsyncHandler<>).MakeGenericType(notificationType),
                type,
                ServiceLifetime.Transient);

            if (!services.Contains(descriptor))
            {
                services.Add(descriptor);
            }
        }
    }

    private Type? GetNotificationType(Type handlerType)
    {
        if (handlerType.IsOfGenericType(typeof(INotificationAsyncHandler<>)))
        {
            Type[] genericArguments = handlerType.BaseType!.GetGenericArguments();

            Type? notificationType = genericArguments.FirstOrDefault(arg => typeof(INotification).IsAssignableFrom(arg));

            if (notificationType is not null)
            {
                return notificationType;
            }
        }

        return null;
    }
}
