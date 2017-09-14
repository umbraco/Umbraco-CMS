using System;
using System.Collections.Generic;
using System.Web;
using Umbraco.Core;
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
    public class HealthCheckResolver : LazyManyObjectsResolverBase<HealthCheckResolver, HealthCheck>, IHealthCheckResolver
    {
        public HealthCheckResolver(ILogger logger, Func<IEnumerable<Type>> lazyTypeList)
            : base(new HealthCheckServiceProvider(), logger, lazyTypeList, ObjectLifetimeScope.Application)
        {
        }

        /// <summary>
        /// Returns all health check instances
        /// </summary>
        public IEnumerable<HealthCheck> HealthChecks
        {
            get { return Values; }
        }

        /// <summary>
        /// This will ctor the HealthCheck instances
        /// </summary>
        /// <remarks>
        /// This is like a super crappy DI - in v8 we have real DI
        /// </remarks>
	    private class HealthCheckServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                var normalArgs = new[] { typeof(HealthCheckContext) };
                var found = serviceType.GetConstructor(normalArgs);
                if (found != null)
                {
                    var gotUmbracoContext = UmbracoContext.Current != null;
                    var healthCheckContext = gotUmbracoContext
                        ? new HealthCheckContext(new HttpContextWrapper(HttpContext.Current), UmbracoContext.Current)
                        : new HealthCheckContext(ApplicationContext.Current);
                    return found.Invoke(new object[]
                    {
                        healthCheckContext
                    });
                }

                //use normal ctor
                return Activator.CreateInstance(serviceType);
            }
        }
    }
}
