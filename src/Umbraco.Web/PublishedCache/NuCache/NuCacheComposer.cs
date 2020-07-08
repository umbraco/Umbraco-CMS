using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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

            var serializer = ConfigurationManager.AppSettings["Umbraco.Web.PublishedCache.NuCache.Serializer"];

            if (serializer == "MsgPack")
            {
                var propertyDictionarySerializer = ConfigurationManager.AppSettings["Umbraco.Web.PublishedCache.NuCache.DictionaryOfPropertiesSerializer"];
                if (propertyDictionarySerializer == "LZ4Map")
                {
                    composition.Register<INucachePropertyOptionsFactory, AppSettingsNucachePropertyMapFactory>();
                    composition.Register(factory =>
                    {
                        var lz4Serializer = factory.GetInstance<Lz4DictionaryOfPropertyDataSerializer>();
                        return new ContentDataSerializer(lz4Serializer);
                    });
                }
                else
                {
                    composition.Register(factory => new ContentDataSerializer(new DictionaryOfPropertyDataSerializer()));
                }
                composition.Register<IContentNestedDataSerializer, MsgPackContentNestedDataSerializer>();
            }
            else
            {
                composition.Register<IContentNestedDataSerializer, JsonContentNestedDataSerializer>();
                composition.Register(factory => new ContentDataSerializer(new DictionaryOfPropertyDataSerializer()));
            }

            // register the NuCache database data source
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
