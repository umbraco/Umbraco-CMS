using LightInject;
using Umbraco.Core.Components;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Plugins;

namespace Umbraco.Web.Strategies.Migrations
{
    public class PostMigrationComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public override void Compose(ServiceContainer container)
        {
            PostMigrationCollectionBuilder.Register(container)
                .AddProducer(factory => factory.GetInstance<PluginManager>().ResolveTypes<IPostMigration>());
        }

        public void Initialize(PostMigrationCollection posts)
        {
            // whatever the runtime level, ie also when installing or upgrading

            foreach (var post in posts)
                MigrationRunner.Migrated += post.Migrated;
        }
    }
}