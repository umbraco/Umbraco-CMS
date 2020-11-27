using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.Builder;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Infrastructure.PublishedCache;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public class NuCacheComposer : ComponentComposer<NuCacheComponent>, IPublishedCacheComposer
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);

            // register the NuCache database data source
            builder.Services.AddTransient<IDataSource, DatabaseDataSource>();

            // register the NuCache published snapshot service
            // must register default options, required in the service ctor
            builder.Services.AddTransient(factory => new PublishedSnapshotServiceOptions());
            builder.SetPublishedSnapshotService<PublishedSnapshotService>();

            // replace this service since we want to improve the content/media
            // mapping lookups if we are using nucache.
            builder.Services.AddUnique<IIdKeyMap>(factory =>
            {
                var idkSvc = new IdKeyMap(factory.GetRequiredService<IScopeProvider>());
                var publishedSnapshotService = factory.GetRequiredService<IPublishedSnapshotService>() as PublishedSnapshotService;
                if (publishedSnapshotService != null)
                {
                    idkSvc.SetMapper(UmbracoObjectTypes.Document, id => publishedSnapshotService.GetDocumentUid(id), uid => publishedSnapshotService.GetDocumentId(uid));
                    idkSvc.SetMapper(UmbracoObjectTypes.Media, id => publishedSnapshotService.GetMediaUid(id), uid => publishedSnapshotService.GetMediaId(uid));
                }
                return idkSvc;
            });

            // add the NuCache health check (hidden from type finder)
            // TODO: no NuCache health check yet
            //composition.HealthChecks().Add<NuCacheIntegrityHealthCheck>();
        }
    }
}
