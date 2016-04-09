using System;

namespace Umbraco.Web.HealthCheck
{
    /// <summary>
    /// Metadata attribute for Health checks
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class HealthCheckAttribute : Attribute
    {        
        public HealthCheckAttribute(string id, string name)
        {
            Id = new Guid(id);
            Name = name;
        }

        public string Name { get; private set; }
        public string Description { get; set; }

        public string Group { get; set; }

        public Guid Id { get; private set; }

        //TODO: Do we need more metadata?
    }
}