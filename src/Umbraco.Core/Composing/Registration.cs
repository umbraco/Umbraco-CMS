using System;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Represents a service registration in the container.
    /// </summary>
    public class Registration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Registration"/> class.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="serviceName"></param>
        public Registration(Type serviceType, string serviceName)
        {
            ServiceType = serviceType;
            ServiceName = serviceName;
        }

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// Gets the service name.
        /// </summary>
        public string ServiceName { get; }
    }
}
