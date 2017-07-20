using LightInject;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Persistence.Migrations
{
    public class MigrationCollectionBuilder : LazyCollectionBuilderBase<MigrationCollectionBuilder, MigrationCollection, IMigration>, IMigrationCollectionBuilder
    {
        public MigrationCollectionBuilder(IServiceContainer container)
            : base(container)
        {
            // because collection builders are "per container" this ctor should run only once per container.
            //
            // note: constructor dependencies do NOT work with lifetimes other than transient
            // see https://github.com/seesharper/LightInject/issues/294
            //
            // resolve ctor dependency from GetInstance() runtimeArguments, if possible - 'factory' is
            // the container, 'info' describes the ctor argument, and 'args' contains the args that
            // were passed to GetInstance() - use first arg if it is the right type.
            //
            // for IMigrationContext
            container.RegisterConstructorDependency((factory, info, args) => args.Length > 0 ? args[0] as IMigrationContext : null);
        }

        protected override void Initialize()
        {
            // nothing - do not register the collection
        }

        protected override MigrationCollectionBuilder This => this;

        // this is *not* needed since we do not register the collection
        // however, keep it here to be absolutely explicit about it
        protected override ILifetime CollectionLifetime { get; } = null; // transient

        public MigrationCollection CreateCollection(IMigrationContext context)
        {
            return new MigrationCollection(CreateItems(context));
        }
    }
}
