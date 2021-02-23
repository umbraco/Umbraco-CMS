using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.Persistence;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/> for the Umbraco's NuCache
    /// </summary>
    public static class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds Umbraco NuCache dependencies
        /// </summary>
        public static IUmbracoBuilder AddNuCache(this IUmbracoBuilder builder)
        {
            // register the NuCache database data source
            builder.Services.TryAddSingleton<INuCacheContentRepository, NuCacheContentRepository>();
            builder.Services.TryAddSingleton<INuCacheContentService, NuCacheContentService>();
            builder.Services.TryAddSingleton<PublishedSnapshotServiceEventHandler>();

            // register the NuCache published snapshot service
            // must register default options, required in the service ctor
            builder.Services.TryAddTransient(factory => new PublishedSnapshotServiceOptions());
            builder.SetPublishedSnapshotService<PublishedSnapshotService>();

            // Add as itself
            builder.Services.TryAddSingleton<PublishedSnapshotService>();
            builder.Services.TryAddSingleton<IPublishedSnapshotStatus, PublishedSnapshotStatus>();

            // replace this service since we want to improve the content/media
            // mapping lookups if we are using nucache.
            // TODO: Gotta wonder how much this does actually improve perf? It's a lot of weird code to make this happen so hope it's worth it
            builder.Services.AddUnique<IIdKeyMap>(factory =>
            {
                var idkSvc = new IdKeyMap(factory.GetRequiredService<IScopeProvider>());
                if (factory.GetRequiredService<IPublishedSnapshotService>() is PublishedSnapshotService publishedSnapshotService)
                {
                    idkSvc.SetMapper(UmbracoObjectTypes.Document, id => publishedSnapshotService.GetDocumentUid(id), uid => publishedSnapshotService.GetDocumentId(uid));
                    idkSvc.SetMapper(UmbracoObjectTypes.Media, id => publishedSnapshotService.GetMediaUid(id), uid => publishedSnapshotService.GetMediaId(uid));
                }

                return idkSvc;
            });

            // add the NuCache health check (hidden from type finder)
            // TODO: no NuCache health check yet
            // composition.HealthChecks().Add<NuCacheIntegrityHealthCheck>();
            return builder;
        }
    }
}
