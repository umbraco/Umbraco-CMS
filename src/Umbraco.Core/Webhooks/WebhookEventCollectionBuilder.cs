using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

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

        // register them - ensuring that each item is registered with the same lifetime as the collection.
        // NOTE: Previously each one was not registered with the same lifetime which would mean that if there
        // was a dependency on an individual item, it would resolve a brand new transient instance which isn't what
        // we would expect to happen. The same item should be resolved from the container as the collection.
        foreach (Type type in types)
        {
            Type notificationType = GetNotificationType(type); // Implement a method to extract the TNotification type from the INotificationHandler

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

    private Type GetNotificationType(Type handlerType)
    {
        if (handlerType.BaseType != null && handlerType.BaseType.IsGenericType && handlerType.BaseType.GetGenericTypeDefinition() == typeof(WebhookEventBase<,>))
        {
            Type[] genericArguments = handlerType.BaseType.GetGenericArguments();

            Type? notificationType = genericArguments.FirstOrDefault(arg => typeof(INotification).IsAssignableFrom(arg));

            if (notificationType is not null)
            {
                return notificationType;
            }
        }

        throw new InvalidOperationException($"Invalid handlerType: {handlerType}");
    }
}
