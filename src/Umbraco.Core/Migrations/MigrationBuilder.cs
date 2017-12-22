using System;
using LightInject;

namespace Umbraco.Core.Migrations
{
    public class MigrationBuilder : IMigrationBuilder
    {
        private readonly IServiceContainer _container;

        public MigrationBuilder(IServiceContainer container)
        {
            _container = container;

            // because the builder should be "per container" this ctor should run only once per container.
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

        public IMigration Build(Type migrationType, IMigrationContext context)
        {
            // LightInject .Create() is a shortcut for .Register() + .GetInstance()
            // but it does not support parameters, so we do it ourselves here

            _container.Register(migrationType);
            return (IMigration) _container.GetInstance(migrationType, new object[] { context });
        }
    }
}
