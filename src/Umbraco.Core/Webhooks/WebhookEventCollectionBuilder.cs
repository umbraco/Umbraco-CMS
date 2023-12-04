using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Webhooks;

public class WebhookEventCollectionBuilder : SetCollectionBuilderBase<WebhookEventCollectionBuilder, WebhookEventCollection, IWebhookEvent>
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

    private void RegisterTypes(IServiceCollection services)
    {
        Type[] types = GetRegisteringTypes(GetTypes()).ToArray();

        // Ensure they are safe
        foreach (Type type in types)
        {
            EnsureType(type, "register");
        }

        // Register all webhooks as notification handlers
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

            services.TryAddEnumerable(descriptor);
        }
    }

    private Type? GetNotificationType(Type handlerType)
        => handlerType.TryGetGenericArguments(typeof(INotificationAsyncHandler<>), out Type[]? genericArguments)
        ? genericArguments.FirstOrDefault(arg => typeof(INotification).IsAssignableFrom(arg))
        : null;
}
