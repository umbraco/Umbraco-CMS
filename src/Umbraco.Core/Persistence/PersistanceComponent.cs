using LightInject;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Runtime;

namespace Umbraco.Core.Persistence
{
    [RequireComponent(typeof(CoreRuntimeComponent))]
    public sealed class PersistanceComponent : UmbracoComponentBase
    {
        public override void Compose(Composition composition)
        {
            var container = composition.Container;

            // register syntax providers - required by database factory
            container.Register<ISqlSyntaxProvider, MySqlSyntaxProvider>("MySqlSyntaxProvider");
            container.Register<ISqlSyntaxProvider, SqlCeSyntaxProvider>("SqlCeSyntaxProvider");
            container.Register<ISqlSyntaxProvider, SqlServerSyntaxProvider>("SqlServerSyntaxProvider");

            // register persistence mappers - required by database factory so needs to be done here
            // means the only place the collection can be modified is in a runtime - afterwards it
            // has been frozen and it is too late
            var mapperCollectionBuilder = container.RegisterCollectionBuilder<MapperCollectionBuilder>();
            ComposeMapperCollection(mapperCollectionBuilder);

            // register database factory - required to check for migrations
            // will be initialized with syntax providers and a logger, and will try to configure
            // from the default connection string name, if possible, else will remain non-configured
            // until properly configured (eg when installing)
            container.RegisterSingleton<IUmbracoDatabaseFactory, UmbracoDatabaseFactory>();
            container.RegisterSingleton(f => f.GetInstance<IUmbracoDatabaseFactory>().SqlContext);
        }

        private void ComposeMapperCollection(MapperCollectionBuilder builder)
        {
            builder.AddCore();
        }

    }
}
