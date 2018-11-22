using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Reflection;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Web.HealthCheck.NotificationMethods;
using Umbraco.Core.Configuration.HealthChecks;

namespace Umbraco.Web.HealthCheck
{


    /// <summary>
    /// Resolves all health check instances
    /// </summary>
    /// <remarks>
    /// Each instance scoped to the lifespan of the http request
    /// </remarks>
    internal class HealthCheckNotificationMethodResolver : LazyManyObjectsResolverBase<HealthCheckNotificationMethodResolver, IHealthCheckNotificatationMethod>, IHealthCheckNotificationMethodsResolver
    {
        public HealthCheckNotificationMethodResolver(ILogger logger, Func<IEnumerable<Type>> lazyTypeList)
            : base(new HealthCheckNotificationMethodServiceProvider(), logger, lazyTypeList, ObjectLifetimeScope.Application)
        {
        }

        /// <summary>
        /// Returns all health check notification method instances
        /// </summary>
        public IEnumerable<IHealthCheckNotificatationMethod> NotificationMethods
        {
            get { return Values; }
        }

        /// <summary>
        /// This will ctor the IHealthCheckNotificatationMethod instances
        /// </summary>
        private class HealthCheckNotificationMethodServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                var ctor = serviceType.GetConstructors().FirstOrDefault();
                if (ctor == null)
                {
                    return null;
                }

                // Load attribute from type in order to find alias for notification method
                var attribute = serviceType.GetCustomAttributes(typeof(HealthCheckNotificationMethodAttribute), true)
                    .FirstOrDefault() as HealthCheckNotificationMethodAttribute;
                if (attribute == null)
                {
                    return null;
                }

                // Using alias, get related configuration
                var healthCheckConfig = UmbracoConfig.For.HealthCheck();
                var notificationMethods = healthCheckConfig.NotificationSettings.NotificationMethods;
                var notificationMethod = notificationMethods[attribute.Alias];


                // Create array for constructor paramenters.  Will consists of common ones that all notification methods have as well
                // as those specific to this particular notification method.
                var baseType = typeof(NotificationMethodBase);
                var baseTypeCtor = baseType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
                var baseTypeCtorParamNames = baseTypeCtor.GetParameters().Select(x => x.Name);

                List<object> ctorParams;

                if (notificationMethod != null)
                {
                    // configuration found so set ctorParams to config values
                    ctorParams = new List<object>
                    {
                        notificationMethod.Enabled,
                        notificationMethod.FailureOnly,
                        notificationMethod.Verbosity
                    };
                    ctorParams.AddRange(ctor.GetParameters()
                        .Where(x => baseTypeCtorParamNames.Contains(x.Name) == false)
                        .Select(x => notificationMethod.Settings[x.Name].Value));
                }
                else
                {
                    // no configuration found so set to default values, enabled = false
                    ctorParams = new List<object> { false, false, HealthCheckNotificationVerbosity.Detailed };
                    ctorParams.AddRange(ctor.GetParameters()
                        .Where(x => baseTypeCtorParamNames.Contains(x.Name) == false)
                        .Select(x => string.Empty));
                }

                // Instantiate the type with the constructor parameters
                return Activator.CreateInstance(serviceType, ctorParams.ToArray());
            }
        }
    }
}
