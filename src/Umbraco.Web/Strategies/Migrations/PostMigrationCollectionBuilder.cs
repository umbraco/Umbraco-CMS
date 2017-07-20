using LightInject;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Strategies.Migrations
{
    public class PostMigrationCollectionBuilder : LazyCollectionBuilderBase<PostMigrationCollectionBuilder, PostMigrationCollection, IPostMigration>
    {
        public PostMigrationCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override PostMigrationCollectionBuilder This => this;

        protected override ILifetime CollectionLifetime => null; // transient
    }
}
