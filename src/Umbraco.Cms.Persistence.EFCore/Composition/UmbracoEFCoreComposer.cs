using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using Umbraco.Cms.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.Composition;

public class UmbracoEFCoreComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IEFCoreMigrationExecutor, EfCoreMigrationExecutor>();

        builder.AddNotificationAsyncHandler<DatabaseSchemaAndDataCreatedNotification, EFCoreCreateTablesNotificationHandler>();
        builder.AddNotificationAsyncHandler<UnattendedInstallNotification, EFCoreCreateTablesNotificationHandler>();
        builder.Services.AddOpenIddict()

            // Register the OpenIddict core components.
            .AddCore(options =>
            {
                options
                    .UseCustomEntityFrameworkCore() // TODO Revert to this after .NET 8 Preview 7: .UseEntityFrameworkCore()
                    .UseDbContext<UmbracoDbContext>();
            });
    }
}


public class EFCoreCreateTablesNotificationHandler : INotificationAsyncHandler<DatabaseSchemaAndDataCreatedNotification>, INotificationAsyncHandler<UnattendedInstallNotification>
{
    private readonly IEFCoreMigrationExecutor _iefCoreMigrationExecutor;

    public EFCoreCreateTablesNotificationHandler(IEFCoreMigrationExecutor iefCoreMigrationExecutor)
    {
        _iefCoreMigrationExecutor = iefCoreMigrationExecutor;
    }

    public async Task HandleAsync(UnattendedInstallNotification notification, CancellationToken cancellationToken)
    {
        await HandleAsync();
    }

    public async Task HandleAsync(DatabaseSchemaAndDataCreatedNotification notification, CancellationToken cancellationToken)
    {
        if (notification.RequiresUpgrade is false)
        {
            await HandleAsync();
        }
    }

    private async Task HandleAsync()
    {
        await _iefCoreMigrationExecutor.ExecuteAllMigrationsAsync();
    }
}


// TODO Revert to this after .NET 8 Preview 7
internal class MyOpenIddictAuthorizationStore<TAuthorization, TApplication, TToken, TContext, TKey> : OpenIddictEntityFrameworkCoreAuthorizationStore<TAuthorization, TApplication, TToken, TContext, TKey>, IOpenIddictAuthorizationStore<TAuthorization>
    where TAuthorization : OpenIddictEntityFrameworkCoreAuthorization<TKey, TApplication, TToken>
    where TApplication : OpenIddictEntityFrameworkCoreApplication<TKey, TAuthorization, TToken>
    where TToken : OpenIddictEntityFrameworkCoreToken<TKey, TApplication, TAuthorization>
    where TContext : DbContext
    where TKey : notnull, IEquatable<TKey>
{
    public MyOpenIddictAuthorizationStore(
        IMemoryCache cache,
        TContext context,
        IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> options)
        : base(cache, context, options)
    {
    }

    private DbSet<TApplication> Applications => Context.Set<TApplication>();

    public override async ValueTask<string?> GetApplicationIdAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        // If the application is not attached to the authorization, try to load it manually.
        if (authorization.Application is null)
        {
            var reference = Context.Entry(authorization).Reference(entry => entry.Application);
            if (reference.EntityEntry.State is EntityState.Detached)
            {
                return null;
            }

            await reference.LoadAsync(cancellationToken: cancellationToken);
        }

        if (authorization.Application is null)
        {
            return null;
        }

        return ConvertIdentifierToString(authorization.Application.Id);
    }

    public override async ValueTask SetApplicationIdAsync(TAuthorization authorization, string? identifier, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        if (!string.IsNullOrEmpty(identifier))
        {
            var key = ConvertIdentifierFromString(identifier);

            authorization.Application = await Applications.AsQueryable()
                .AsTracking()
                .FirstOrDefaultAsync(application => application.Id!.Equals(key), cancellationToken) ??
                throw new InvalidOperationException();
        }

        else
        {
            // If the application is not attached to the authorization, try to load it manually.
            if (authorization.Application is null)
            {
                var reference = Context.Entry(authorization).Reference(entry => entry.Application);
                if (reference.EntityEntry.State is EntityState.Detached)
                {
                    return;
                }

                await reference.LoadAsync(cancellationToken: cancellationToken);
            }

            authorization.Application = null;
        }
    }
}

// TODO Revert to this after .NET 8 Preview 7
internal class MyOpenIddictAuthorizationStoreResolver : IOpenIddictAuthorizationStoreResolver
{
    private readonly TypeResolutionCache _cache;
    private readonly IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> _options;
    private readonly IServiceProvider _provider;

    public MyOpenIddictAuthorizationStoreResolver(
        TypeResolutionCache cache,
        IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> options,
        IServiceProvider provider)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public IOpenIddictAuthorizationStore<TAuthorization> Get<TAuthorization>() where TAuthorization : class
    {
        var store = _provider.GetService<IOpenIddictAuthorizationStore<TAuthorization>>();
        if (store is not null)
        {
            return store;
        }

        var type = _cache.GetOrAdd(typeof(TAuthorization), key =>
        {
            var root = OpenIddictHelpers.FindGenericBaseType(key, typeof(OpenIddictEntityFrameworkCoreAuthorization<,,>)) ??
                throw new InvalidOperationException();
            var context = _options.CurrentValue.DbContextType ??
                throw new InvalidOperationException();
            return typeof(MyOpenIddictAuthorizationStore<,,,,>).MakeGenericType(
                /* TAuthorization: */ key,
                /* TApplication: */ root.GenericTypeArguments[1],
                /* TToken: */ root.GenericTypeArguments[2],
                /* TContext: */ context,
                /* TKey: */ root.GenericTypeArguments[0]);
        });

        return (IOpenIddictAuthorizationStore<TAuthorization>)_provider.GetRequiredService(type);
    }

    [SuppressMessage("Design", "CA1034:Nested types should not be visible")]
    public sealed class TypeResolutionCache : ConcurrentDictionary<Type, Type> { }
}

// TODO Revert to this after .NET 8 Preview 7
internal class MyOpenIddictTokenStore<TToken, TApplication, TAuthorization, TContext, TKey> : OpenIddictEntityFrameworkCoreTokenStore<TToken, TApplication, TAuthorization, TContext, TKey>, IOpenIddictTokenStore<TToken>
    where TToken : OpenIddictEntityFrameworkCoreToken<TKey, TApplication, TAuthorization>
    where TApplication : OpenIddictEntityFrameworkCoreApplication<TKey, TAuthorization, TToken>
    where TAuthorization : OpenIddictEntityFrameworkCoreAuthorization<TKey, TApplication, TToken>
    where TContext : DbContext
    where TKey : notnull, IEquatable<TKey>
{
    public MyOpenIddictTokenStore(
        IMemoryCache cache,
        TContext context,
        IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> options)
        : base(cache, context, options)
    {
    }

    private DbSet<TApplication> Applications => Context.Set<TApplication>();
    private DbSet<TAuthorization> Authorizations => Context.Set<TAuthorization>();

    public override async ValueTask<string?> GetApplicationIdAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        // If the application is not attached to the token, try to load it manually.
        if (token.Application is null)
        {
            var reference = Context.Entry(token).Reference(entry => entry.Application);
            if (reference.EntityEntry.State is EntityState.Detached)
            {
                return null;
            }

            await reference.LoadAsync(cancellationToken: cancellationToken);
        }

        if (token.Application is null)
        {
            return null;
        }

        return ConvertIdentifierToString(token.Application.Id);
    }

    public override async ValueTask<string?> GetAuthorizationIdAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        // If the authorization is not attached to the token, try to load it manually.
        if (token.Authorization is null)
        {
            var reference = Context.Entry(token).Reference(entry => entry.Authorization);
            if (reference.EntityEntry.State is EntityState.Detached)
            {
                return null;
            }

            await reference.LoadAsync(cancellationToken: cancellationToken);
        }

        if (token.Authorization is null)
        {
            return null;
        }

        return ConvertIdentifierToString(token.Authorization.Id);
    }

    public override async ValueTask SetApplicationIdAsync(TToken token, string? identifier, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (!string.IsNullOrEmpty(identifier))
        {
            var key = ConvertIdentifierFromString(identifier);

            // Warning: FindAsync() is deliberately not used to work around a breaking change introduced
            // in Entity Framework Core 3.x (where a ValueTask instead of a Task is now returned).
            token.Application = await Applications.AsQueryable()
                .AsTracking()
                .FirstOrDefaultAsync(application => application.Id!.Equals(key), cancellationToken) ??
                throw new InvalidOperationException();
        }

        else
        {
            // If the application is not attached to the token, try to load it manually.
            if (token.Application is null)
            {
                var reference = Context.Entry(token).Reference(entry => entry.Application);
                if (reference.EntityEntry.State is EntityState.Detached)
                {
                    return;
                }

                await reference.LoadAsync(cancellationToken: cancellationToken);
            }

            token.Application = null;
        }
    }

    public override async ValueTask SetAuthorizationIdAsync(TToken token, string? identifier, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (!string.IsNullOrEmpty(identifier))
        {
            var key = ConvertIdentifierFromString(identifier);

            // Warning: FindAsync() is deliberately not used to work around a breaking change introduced
            // in Entity Framework Core 3.x (where a ValueTask instead of a Task is now returned).
            token.Authorization = await Authorizations.AsQueryable()
                .AsTracking()
                .FirstOrDefaultAsync(authorization => authorization.Id!.Equals(key), cancellationToken) ??
                throw new InvalidOperationException();
        }

        else
        {
            // If the authorization is not attached to the token, try to load it manually.
            if (token.Authorization is null)
            {
                var reference = Context.Entry(token).Reference(entry => entry.Authorization);
                if (reference.EntityEntry.State is EntityState.Detached)
                {
                    return;
                }

                await reference.LoadAsync(cancellationToken: cancellationToken);
            }

            token.Authorization = null;
        }
    }
}

// TODO Revert to this after .NET 8 Preview 7
internal class MyOpenIddictTokenStoreResolver : IOpenIddictTokenStoreResolver
{
    private readonly TypeResolutionCache _cache;
    private readonly IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> _options;
    private readonly IServiceProvider _provider;

    public MyOpenIddictTokenStoreResolver(
        TypeResolutionCache cache,
        IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> options,
        IServiceProvider provider)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public IOpenIddictTokenStore<TToken> Get<TToken>() where TToken : class
    {
        var store = _provider.GetService<IOpenIddictTokenStore<TToken>>();
        if (store is not null)
        {
            return store;
        }

        var type = _cache.GetOrAdd(typeof(TToken), key =>
        {
            var root = OpenIddictHelpers.FindGenericBaseType(key, typeof(OpenIddictEntityFrameworkCoreToken<,,>)) ??
                throw new InvalidOperationException();
            var context = _options.CurrentValue.DbContextType ??
                throw new InvalidOperationException();
            return typeof(MyOpenIddictTokenStore<,,,,>).MakeGenericType(
                /* TToken: */ key,
                /* TApplication: */ root.GenericTypeArguments[1],
                /* TAuthorization: */ root.GenericTypeArguments[2],
                /* TContext: */ context,
                /* TKey: */ root.GenericTypeArguments[0]);
        });

        return (IOpenIddictTokenStore<TToken>)_provider.GetRequiredService(type);
    }

    [SuppressMessage("Design", "CA1034:Nested types should not be visible")]
    public sealed class TypeResolutionCache : ConcurrentDictionary<Type, Type> { }
}

// TODO Revert to this after .NET 8 Preview 7
internal static class OpenIddictHelpers
{
    public static Type FindGenericBaseType(Type type, Type definition)
        => FindGenericBaseTypes(type, definition).FirstOrDefault()!;

    public static IEnumerable<Type> FindGenericBaseTypes(Type type, Type definition)
    {
        ArgumentNullException.ThrowIfNull(type);

        ArgumentNullException.ThrowIfNull(definition);

        if (!definition.IsGenericTypeDefinition)
        {
            throw new ArgumentException(null, nameof(definition));
        }

        if (definition.IsInterface)
        {
            foreach (var contract in type.GetInterfaces())
            {
                if (!contract.IsGenericType && !contract.IsConstructedGenericType)
                {
                    continue;
                }

                if (contract.GetGenericTypeDefinition() == definition)
                {
                    yield return contract;
                }
            }
        }

        else
        {
            for (var candidate = type; candidate is not null; candidate = candidate.BaseType)
            {
                if (!candidate.IsGenericType && !candidate.IsConstructedGenericType)
                {
                    continue;
                }

                if (candidate.GetGenericTypeDefinition() == definition)
                {
                    yield return candidate;
                }
            }
        }
    }
}

// TODO Revert to this after .NET 8 Preview 7
internal static class OpenIddictExtension
{
    public static OpenIddictEntityFrameworkCoreBuilder UseCustomEntityFrameworkCore(this OpenIddictCoreBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Since Entity Framework Core may be used with databases performing case-insensitive
        // or culture-sensitive comparisons, ensure the additional filtering logic is enforced
        // in case case-sensitive stores were registered before this extension was called.
        builder.Configure(options => options.DisableAdditionalFiltering = false);

        builder.SetDefaultApplicationEntity<OpenIddictEntityFrameworkCoreApplication>()
               .SetDefaultAuthorizationEntity<OpenIddictEntityFrameworkCoreAuthorization>()
               .SetDefaultScopeEntity<OpenIddictEntityFrameworkCoreScope>()
               .SetDefaultTokenEntity<OpenIddictEntityFrameworkCoreToken>();

        builder.ReplaceApplicationStoreResolver<OpenIddictEntityFrameworkCoreApplicationStoreResolver>()
               .ReplaceAuthorizationStoreResolver<MyOpenIddictAuthorizationStoreResolver>()
               .ReplaceScopeStoreResolver<OpenIddictEntityFrameworkCoreScopeStoreResolver>()
               .ReplaceTokenStoreResolver<MyOpenIddictTokenStoreResolver>();

        builder.Services.TryAddSingleton<OpenIddictEntityFrameworkCoreApplicationStoreResolver.TypeResolutionCache>();
        builder.Services.TryAddSingleton<MyOpenIddictAuthorizationStoreResolver.TypeResolutionCache>();
        builder.Services.TryAddSingleton<OpenIddictEntityFrameworkCoreScopeStoreResolver.TypeResolutionCache>();
        builder.Services.TryAddSingleton<MyOpenIddictTokenStoreResolver.TypeResolutionCache>();

        builder.Services.TryAddScoped(typeof(OpenIddictEntityFrameworkCoreApplicationStore<,,,,>));
        builder.Services.TryAddScoped(typeof(MyOpenIddictAuthorizationStore<,,,,>));
        builder.Services.TryAddScoped(typeof(OpenIddictEntityFrameworkCoreScopeStore<,,>));
        builder.Services.TryAddScoped(typeof(MyOpenIddictTokenStore<,,,,>));

        return new OpenIddictEntityFrameworkCoreBuilder(builder.Services);
    }
}
