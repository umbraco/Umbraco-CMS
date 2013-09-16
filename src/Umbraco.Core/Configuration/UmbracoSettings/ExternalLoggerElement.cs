using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ExternalLoggerElement : ConfigurationElement
    {
        [ConfigurationProperty("assembly")]
        internal string Assembly
        {
            get { return (string)base["assembly"]; }
        }

        [ConfigurationProperty("type")]
        internal string Type
        {
            get { return (string)base["type"]; }
        }

        [ConfigurationProperty("logAuditTrail")]
        internal bool LogAuditTrail
        {
            get { return (bool)base["logAuditTrail"]; }
        }
    }
}