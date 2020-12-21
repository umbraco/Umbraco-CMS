using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Models;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Infrastructure.PublishedCache;
using Umbraco.Infrastructure.PublishedCache.Persistence;

namespace Umbraco.Web.PublishedCache.NuCache
{
    // TODO: We'll need to change this stuff to IUmbracoBuilder ext and control the order of things there,
    // see comment in ModelsBuilderComposer which requires this weird IPublishedCacheComposer
    public class NuCacheComposer : IComposer, IPublishedCacheComposer
    {
        /// <inheritdoc/>
        public void Compose(IUmbracoBuilder builder)
        {
            // register the NuCache database data source
            builder.Services.AddSingleton<INuCacheContentRepository, NuCacheContentRepository>();
            builder.Services.AddSingleton<INuCacheContentService, NuCacheContentService>();
            builder.Services.AddSingleton<PublishedSnapshotServiceEventHandler>();

            // register the NuCache published snapshot service
            // must register default options, required in the service ctor
            builder.Services.AddTransient(factory => new PublishedSnapshotServiceOptions());
            builder.SetPublishedSnapshotService<PublishedSnapshotService>();

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
        }
    }
}
