using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Web.HealthCheck
{
    /// <summary>
    /// Resolves all health check instances
    /// </summary>
    /// <remarks>
    /// Each instance scoped to the lifespan of the http request
    /// </remarks>
    internal class HealthCheckResolver : LazyManyObjectsResolverBase<HealthCheckResolver, HealthCheck>, IHealthCheckResolver
    {
        public HealthCheckResolver(ILogger logger, Func<IEnumerable<Type>> lazyTypeList) 
            : base(new HealthCheckServiceProvider(), logger, lazyTypeList, ObjectLifetimeScope.HttpRequest)
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
                    return found.Invoke(new object[]
                    {
                        new HealthCheckContext(new HttpContextWrapper(HttpContext.Current), UmbracoContext.Current)
                    });
                }
                    
                //use normal ctor
                return Activator.CreateInstance(serviceType);
            }
        }
    }
}
