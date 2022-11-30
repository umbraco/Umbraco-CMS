using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Extensions;

public static class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Registers all <see cref="INotificationHandler{TNotification}" /> within an assembly
    /// </summary>
    /// <param name="self">
    ///     <see cref="IUmbracoBuilder" />
    /// </param>
    /// <typeparam name="T">Type contained within the targeted assembly</typeparam>
    /// <returns></returns>
    public static IUmbracoBuilder AddNotificationsFromAssembly<T>(this IUmbracoBuilder self)
    {
        AddNotificationHandlers<T>(self);
        AddAsyncNotificationHandlers<T>(self);

        return self;
    }

    private static void AddNotificationHandlers<T>(IUmbracoBuilder self)
    {
        List<Type> notificationHandlers = GetNotificationHandlers<T>();
        foreach (Type notificationHandler in notificationHandlers)
        {
            List<Type> handlerImplementations = GetNotificationHandlerImplementations<T>(notificationHandler);
            foreach (Type implementation in handlerImplementations)
            {
                RegisterNotificationHandler(self, implementation, notificationHandler);
            }
        }
    }

    private static List<Type> GetNotificationHandlers<T>() =>
        typeof(T).Assembly.GetTypes()
            .Where(x => x.IsAssignableToGenericType(typeof(INotificationHandler<>)))
            .ToList();

    private static List<Type> GetNotificationHandlerImplementations<T>(Type handlerType) =>
        handlerType
            .GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
            .ToList();

    private static void AddAsyncNotificationHandlers<T>(IUmbracoBuilder self)
    {
        List<Type> notificationHandlers = GetAsyncNotificationHandlers<T>();
        foreach (Type notificationHandler in notificationHandlers)
        {
            List<Type> handlerImplementations = GetAsyncNotificationHandlerImplementations<T>(notificationHandler);
            foreach (Type handler in handlerImplementations)
            {
                RegisterNotificationHandler(self, handler, notificationHandler);
            }
        }
    }

    private static List<Type> GetAsyncNotificationHandlers<T>() =>
        typeof(T).Assembly.GetTypes()
            .Where(x => x.IsAssignableToGenericType(typeof(INotificationAsyncHandler<>)))
            .ToList();

    private static List<Type> GetAsyncNotificationHandlerImplementations<T>(Type handlerType) =>
        handlerType
            .GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(INotificationAsyncHandler<>))
            .ToList();

    private static void RegisterNotificationHandler(IUmbracoBuilder self, Type notificationHandlerType, Type implementingHandlerType)
    {
        var descriptor =
            new UniqueServiceDescriptor(notificationHandlerType, implementingHandlerType, ServiceLifetime.Transient);
        if (!self.Services.Contains(descriptor))
        {
            self.Services.Add(descriptor);
        }
    }

    private static bool IsAssignableToGenericType(this Type givenType, Type genericType)
    {
        Type[] interfaceTypes = givenType.GetInterfaces();

        foreach (Type it in interfaceTypes)
        {
            if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }
        }

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
        {
            return true;
        }

        Type? baseType = givenType.BaseType;
        return baseType != null && IsAssignableToGenericType(baseType, genericType);
    }
}
