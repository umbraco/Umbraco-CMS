using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Webhooks;

/// <summary>
/// Builder for the <see cref="WebhookEventCollection"/> that handles registration of webhook events
/// and their corresponding notification handlers with the dependency injection container.
/// </summary>
public class WebhookEventCollectionBuilder : SetCollectionBuilderBase<WebhookEventCollectionBuilder, WebhookEventCollection, IWebhookEvent>
{
    /// <inheritdoc />
    protected override WebhookEventCollectionBuilder This => this;

    /// <inheritdoc />
    public override void RegisterWith(IServiceCollection services)
    {
        // register the collection
        services.Add(new ServiceDescriptor(typeof(WebhookEventCollection), CreateCollection, ServiceLifetime.Singleton));

        // register the types
        RegisterTypes(services);
        base.RegisterWith(services);
    }

    /// <summary>
    /// Registers webhook event types and their notification handlers with the service collection.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
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

    /// <summary>
    /// Gets the notification type that a webhook event handler handles.
    /// </summary>
    /// <param name="handlerType">The webhook event handler type.</param>
    /// <returns>The notification type, or <c>null</c> if not found.</returns>
    private Type? GetNotificationType(Type handlerType)
        => handlerType.TryGetGenericArguments(typeof(INotificationAsyncHandler<>), out Type[]? genericArguments)
        ? genericArguments.FirstOrDefault(arg => typeof(INotification).IsAssignableFrom(arg))
        : null;
}
