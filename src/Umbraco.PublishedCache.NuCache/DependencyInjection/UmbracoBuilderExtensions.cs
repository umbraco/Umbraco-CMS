using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Cms.Infrastructure.PublishedCache.Persistence;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder" /> for the Umbraco's NuCache
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds Umbraco NuCache dependencies
    /// </summary>
    public static IUmbracoBuilder AddNuCache(this IUmbracoBuilder builder)
    {
        builder.Services.TryAddSingleton<INuCacheContentRepository, NuCacheContentRepository>();
        builder.Services.TryAddSingleton<INuCacheContentService, NuCacheContentService>();
        builder.Services.TryAddSingleton<PublishedSnapshotServiceEventHandler>();

        // register the NuCache published snapshot service
        // must register default options, required in the service ctor
        builder.Services.TryAddTransient(factory => new PublishedSnapshotServiceOptions());
        builder.SetPublishedSnapshotService<PublishedSnapshotService>();
        builder.Services.TryAddSingleton<IPublishedSnapshotStatus, PublishedSnapshotStatus>();
        builder.Services.TryAddTransient<IReservedFieldNamesService, ReservedFieldNamesService>();

        builder.Services.AddUnique<IIdKeyMap, IdKeyMap>();

        builder.AddNuCacheNotifications();

        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, NuCacheStartupHandler>();
        builder.Services.AddSingleton<IContentCacheDataSerializerFactory>(s =>
        {
            IOptions<NuCacheSettings> options = s.GetRequiredService<IOptions<NuCacheSettings>>();
            switch (options.Value.NuCacheSerializerType)
            {
                case NuCacheSerializerType.JSON:
                    return new JsonContentNestedDataSerializerFactory();
                case NuCacheSerializerType.MessagePack:
                    return ActivatorUtilities.CreateInstance<MsgPackContentNestedDataSerializerFactory>(s);
                default:
                    throw new IndexOutOfRangeException();
            }
        });

        builder.Services.AddSingleton<IPropertyCacheCompressionOptions>(s =>
        {
            IOptions<NuCacheSettings> options = s.GetRequiredService<IOptions<NuCacheSettings>>();

            if (options.Value.NuCacheSerializerType == NuCacheSerializerType.MessagePack &&
                options.Value.UnPublishedContentCompression)
            {
                return new UnPublishedContentPropertyCacheCompressionOptions();
            }

            return new NoopPropertyCacheCompressionOptions();
        });

        builder.Services.AddSingleton(s => new ContentDataSerializer(new DictionaryOfPropertyDataSerializer()));

        // add the NuCache health check (hidden from type finder)
        // TODO: no NuCache health check yet
        // composition.HealthChecks().Add<NuCacheIntegrityHealthCheck>();
        return builder;
    }

    private static IUmbracoBuilder AddNuCacheNotifications(this IUmbracoBuilder builder)
    {
        builder
            .AddNotificationHandler<LanguageSavedNotification, PublishedSnapshotServiceEventHandler>()
            .AddNotificationHandler<MemberDeletingNotification, PublishedSnapshotServiceEventHandler>()
#pragma warning disable CS0618 // Type or member is obsolete
            .AddNotificationHandler<ContentRefreshNotification, PublishedSnapshotServiceEventHandler>()
            .AddNotificationHandler<MediaRefreshNotification, PublishedSnapshotServiceEventHandler>()
            .AddNotificationHandler<MemberRefreshNotification, PublishedSnapshotServiceEventHandler>()
            .AddNotificationHandler<ContentTypeRefreshedNotification, PublishedSnapshotServiceEventHandler>()
            .AddNotificationHandler<MediaTypeRefreshedNotification, PublishedSnapshotServiceEventHandler>()
            .AddNotificationHandler<MemberTypeRefreshedNotification, PublishedSnapshotServiceEventHandler>()
            .AddNotificationHandler<ScopedEntityRemoveNotification, PublishedSnapshotServiceEventHandler>()
#pragma warning restore CS0618 // Type or member is obsolete
            ;

        return builder;
    }
}
