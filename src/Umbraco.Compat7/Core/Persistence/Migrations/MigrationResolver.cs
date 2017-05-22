using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;
using CoreCurrent = Umbraco.Core.DI.Current;
using LightInject;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.Persistence.Migrations
{
    public class MigrationResolver : LazyManyObjectsResolverBase<MigrationCollectionBuilder, MigrationCollection, IMigration>
    {
        private MigrationResolver(MigrationCollectionBuilder builder)
            : base(builder)
        { }

        public static MigrationResolver Current { get; }
            = new MigrationResolver(CoreCurrent.Container.GetInstance<MigrationCollectionBuilder>());

        public IEnumerable<IMigration> GetMigrations(IMigrationContext context) => Builder.CreateCollection(context);
    }
}
