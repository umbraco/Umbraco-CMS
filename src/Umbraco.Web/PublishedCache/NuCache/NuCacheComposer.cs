using System.Configuration;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Sync;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public class NuCacheComposer : ComponentComposer<NuCacheComponent>, ICoreComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            //Overriden on Run state in DatabaseServerRegistrarAndMessengerComposer
            composition.Register<ISyncBootStateAccessor, NonRuntimeLevelBootStateAccessor>(Lifetime.Singleton);

            var serializer = ConfigurationManager.AppSettings[NuCacheSerializerComponent.Nucache_Serializer_Key];
            if (serializer != "MsgPack")
            {
                // TODO: This allows people to revert to the legacy serializer, by default it will be MessagePack
                composition.RegisterUnique<IContentCacheDataSerializerFactory, JsonContentNestedDataSerializerFactory>();
            }
            else
            {
                composition.RegisterUnique<IContentCacheDataSerializerFactory, MsgPackContentNestedDataSerializerFactory>();
            }
            var unPublishedContentCompression = ConfigurationManager.AppSettings[NuCacheSerializerComponent.Nucache_UnPublishedContentCompression_Key];
            if (serializer == "MsgPack" && unPublishedContentCompression == "true")
            {
                composition.RegisterUnique<IPropertyCacheCompressionOptions, UnPublishedContentPropertyCacheCompressionOptions>();
            }
            else
            {
                composition.RegisterUnique<IPropertyCacheCompressionOptions, NoopPropertyCacheCompressionOptions>();
            }


            composition.RegisterUnique(factory => new ContentDataSerializer(new DictionaryOfPropertyDataSerializer()));

            //Overriden on Run state in DatabaseServerRegistrarAndMessengerComposer
            composition.Register<ISyncBootStateAccessor, NonRuntimeLevelBootStateAccessor>(Lifetime.Singleton);

            // register the NuCache database data source
            composition.RegisterUnique<IDataSource, DatabaseDataSource>();

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
