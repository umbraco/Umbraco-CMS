using System.Configuration;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public class NuCacheComposer : ComponentComposer<NuCacheComponent>, ICoreComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            // register the NuCache database data source
            var disallowUnpublishedContent = ConfigurationManager.AppSettings["Umbraco.Web.PublishedCache.NuCache.NoUnpublishedContentData"];
            if (disallowUnpublishedContent == "true")
            {
                var dataSourceConfig = new DatabaseDataSourceConfiguration(true);
                composition.RegisterUnique<DatabaseDataSourceConfiguration>(dataSourceConfig);
            }
            else
            {
                composition.RegisterUnique<DatabaseDataSourceConfiguration, DatabaseDataSourceConfiguration>();
            }
            composition.Register<IDataSource, DatabaseDataSource>();

            // register the NuCache published snapshot service
            // must register default options, required in the service ctor
            composition.Register(factory => new PublishedSnapshotServiceOptions());
            composition.SetPublishedSnapshotService<PublishedSnapshotService>();

            // add the NuCache health check (hidden from type finder)
            // TODO: no NuCache health check yet
            //composition.HealthChecks().Add<NuCacheIntegrityHealthCheck>();
        }
    }
}
