using CSharpTest.Net.Serialization;
using System.Collections.Generic;
using System.Configuration;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public class NuCacheComposer : ComponentComposer<NuCacheComponent>, ICoreComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            var serializer = ConfigurationManager.AppSettings["Umbraco.Web.PublishedCache.NuCache.Serializer"];
            if (serializer != "MsgPack")
            {
                // TODO: This allows people to revert to the legacy serializer, by default it will be MessagePack
                composition.RegisterUnique<IContentCacheDataSerializerFactory, JsonContentNestedDataSerializerFactory>();
            }
            else
            {
                composition.RegisterUnique<IContentCacheDataSerializerFactory, MsgPackContentNestedDataSerializerFactory>();
            }

            composition.RegisterUnique<IPropertyCacheCompressionOptions, NoopPropertyCacheCompressionOptions>();

            RegisterBPlusTreeSerializers(composition);

            composition.RegisterUnique<ITransactableDictionaryFactory<int, ContentNodeKit>, BPlusTreeTransactableDictionaryFactory<int, ContentNodeKit>>();

            composition.RegisterUnique<INucacheRepositoryFactory, TransactableDictionaryNucacheRepositoryFactory>();

            composition.Register<INucacheContentRepository>(factory => factory.GetInstance<INucacheRepositoryFactory>().GetContentRepository()
            , Lifetime.Singleton);
            composition.Register<INucacheMediaRepository>(factory => factory.GetInstance<INucacheRepositoryFactory>().GetMediaRepository()
            , Lifetime.Singleton);

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

        private static void RegisterBPlusTreeSerializers(Composition composition)
        {
            composition.RegisterUnique<ISerializer<IDictionary<string, PropertyData[]>>, DictionaryOfPropertyDataSerializer>();
            composition.RegisterUnique<ISerializer<IReadOnlyDictionary<string, CultureVariation>>, DictionaryOfCultureVariationSerializer>();
            composition.RegisterUnique<ISerializer<ContentData>, ContentDataSerializer>();
            composition.RegisterUnique<ISerializer<ContentNodeKit>, ContentNodeKitSerializer>();
            composition.RegisterUnique<ISerializer<int>, PrimitiveSerializer>();

            composition.RegisterUnique<ITransactableDictionarySerializer<int>, BPlusTreeTransactableDictionarySerializerAdapter<int>>(); //Key Serializer
            composition.RegisterUnique<ITransactableDictionarySerializer<ContentNodeKit>, BPlusTreeTransactableDictionarySerializerAdapter<ContentNodeKit>>(); // Value Serializer
        }
    }
}
