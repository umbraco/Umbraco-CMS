using LightInject;
using Umbraco.Core.DependencyInjection;

namespace Umbraco.Web.HealthCheck
{
    public class HealthCheckCollectionBuilder : LazyCollectionBuilderBase<HealthCheckCollectionBuilder, HealthCheckCollection, HealthCheck>
    {
        public HealthCheckCollectionBuilder(IServiceContainer container)
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
            // for HealthCheckContext
            container.RegisterConstructorDependency((factory, info, args) => args.Length > 0 ? args[0] as HealthCheckContext : null);
        }

        protected override HealthCheckCollectionBuilder This => this;

        protected override void Initialize()
        {
            // nothing - do not register the collection
        }

        public HealthCheckCollection CreateCollection(HealthCheckContext context)
        {
            return new HealthCheckCollection(CreateItems(context));
        }
    }
}
