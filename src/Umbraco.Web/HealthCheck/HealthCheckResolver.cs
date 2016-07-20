using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            : base(container, logger, types, ObjectLifetimeScope.HttpRequest)
        { }

        protected override IEnumerable<HealthCheck> CreateValues(ObjectLifetimeScope scope)
        {
            EnsureTypesRegisterred(scope, container =>
            {
                // resolve ctor dependency from GetInstance() runtimeArguments, if possible - 'factory' is
                // the container, 'info' describes the ctor argument, and 'args' contains the args that
                // were passed to GetInstance() - use first arg if it is the right type,
                //
                // for HealthCheckContext
                container.RegisterConstructorDependency((factory, info, args) => args.Length > 0 ? args[0] as HealthCheckContext : null);
            });

            return InstanceTypes.Select(x => (HealthCheck) Container.GetInstance(x, new object[] { _healthCheckContext }));
        }

        private HealthCheckContext _healthCheckContext;

        public IEnumerable<HealthCheck> GetHealthChecks(HealthCheckContext context)
        {
            _healthCheckContext = context;
            return Values;
        }
    }
}
