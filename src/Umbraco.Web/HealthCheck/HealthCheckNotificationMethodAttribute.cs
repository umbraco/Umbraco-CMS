using System;

namespace Umbraco.Web.HealthCheck
{
    /// <summary>
    /// Metadata attribute for health check notification methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class HealthCheckNotificationMethodAttribute : Attribute
    {
        public HealthCheckNotificationMethodAttribute(string alias)
        {
            Alias = alias;
        }

        public string Alias { get; }
    }
}
