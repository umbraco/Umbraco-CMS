using System;
using System.Collections.Generic;
using System.Linq;
using LightInject;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.HealthCheck
{
    /// <summary>
    /// Resolves all health check instances
    /// </summary>
    /// <remarks>
    /// Each instance scoped to the lifespan of the http request
    /// </remarks>
    internal class HealthCheckResolver : ContainerLazyManyObjectsResolver<HealthCheckResolver, HealthCheck>, IHealthCheckResolver
    {
        public HealthCheckResolver(IServiceContainer container, ILogger logger, Func<IEnumerable<Type>> types)
            : base(container, logger, types, ObjectLifetimeScope.Transient) // do NOT change .Transient, see CreateValues below
        { }

        protected override IEnumerable<HealthCheck> CreateValues(ObjectLifetimeScope scope)
        {
            // note: constructor dependencies do NOT work with lifetimes other than transient
            // see https://github.com/seesharper/LightInject/issues/294
            EnsureTypesRegisterred(scope, container =>
            {
                // resolve ctor dependency from GetInstance() runtimeArguments, if possible - 'factory' is
                // the container, 'info' describes the ctor argument, and 'args' contains the args that
                // were passed to GetInstance() - use first arg if it is the right type,
                //
                // for HealthCheckContext
                container.RegisterConstructorDependency((factory, info, args) => args.Length > 0 ? args[0] as HealthCheckContext : null);
            });

            var arg = new object[] { _healthCheckContext };
            return InstanceTypes.Select(x => (HealthCheck) Container.GetInstance(x, arg));
        }

        private HealthCheckContext _healthCheckContext;

        public IEnumerable<HealthCheck> GetHealthChecks(HealthCheckContext context)
        {
            _healthCheckContext = context;
            return Values;
        }
    }
}
