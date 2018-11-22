using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.HealthCheck
{
    /// <summary>
    /// Provides a base class for health checks.
    /// </summary>
    [DataContract(Name = "healtCheck", Namespace = "")]
    public abstract class HealthCheck : IDiscoverable
    {
        protected HealthCheck()
        {
            //Fill in the metadata
            var thisType = GetType();
            var meta = thisType.GetCustomAttribute<HealthCheckAttribute>(false);
            if (meta == null)
                throw new InvalidOperationException($"The health check {thisType} requires a {typeof (HealthCheckAttribute)}");
            Name = meta.Name;
            Description = meta.Description;
            Group = meta.Group;
            Id = meta.Id;
        }

        [DataMember(Name = "id")]
        public Guid Id { get; private set; }

        [DataMember(Name = "name")]
        public string Name { get; private set; }

        [DataMember(Name = "description")]
        public string Description { get; private set; }

        [DataMember(Name = "group")]
        public string Group { get; private set; }

        /// <summary>
        /// Get the status for this health check
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<HealthCheckStatus> GetStatus();

        /// <summary>
        /// Executes the action and returns it's status
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public abstract HealthCheckStatus ExecuteAction(HealthCheckAction action);

        //TODO: What else?
    }
}
