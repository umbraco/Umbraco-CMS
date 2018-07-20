using Umbraco.Core.Composing;

namespace Umbraco.Core.Migrations
{
    public class PostMigrationCollectionBuilder : LazyCollectionBuilderBase<PostMigrationCollectionBuilder, PostMigrationCollection, IPostMigration>
    {
        public PostMigrationCollectionBuilder(IContainer container)
            : base(container)
        { }

        protected override PostMigrationCollectionBuilder This => this;

        protected override Lifetime CollectionLifetime => Lifetime.Transient;
    }
}
