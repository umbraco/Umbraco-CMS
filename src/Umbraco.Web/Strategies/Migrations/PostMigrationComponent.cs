using Umbraco.Core.Components;
using Umbraco.Core.DI;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Plugins;
using LightInject;

namespace Umbraco.Web.Strategies.Migrations
{
    public class PostMigrationComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public override void Compose(Composition composition)
        {
            composition.Container.RegisterCollectionBuilder<PostMigrationCollectionBuilder>()
                .Add(factory => factory.GetInstance<PluginManager>().ResolveTypes<IPostMigration>());
        }

        public void Initialize(PostMigrationCollection posts)
        {
            // whatever the runtime level, ie also when installing or upgrading

            foreach (var post in posts)
                MigrationRunner.Migrated += post.Migrated;
        }
    }

    public static class PostMigrationComponentCompositionExtensions
    {
        /// <summary>
        /// Gets the post-migrations collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        internal static PostMigrationCollectionBuilder PostMigrations(this Composition composition)
            => composition.Container.GetInstance<PostMigrationCollectionBuilder>();
    }
}