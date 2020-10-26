using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
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
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            // register the NuCache database data source
            composition.Services.AddTransient<IDataSource, DatabaseDataSource>();

            // register the NuCache published snapshot service
            // must register default options, required in the service ctor
            composition.Register(factory => new PublishedSnapshotServiceOptions());
            composition.SetPublishedSnapshotService<PublishedSnapshotService>();

            // replace this service since we want to improve the content/media
            // mapping lookups if we are using nucache.
            composition.RegisterUnique<IIdKeyMap>(factory =>
            {
                var idkSvc = new IdKeyMap(factory.GetInstance<IScopeProvider>());
                var publishedSnapshotService = factory.GetInstance<IPublishedSnapshotService>() as PublishedSnapshotService;
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
