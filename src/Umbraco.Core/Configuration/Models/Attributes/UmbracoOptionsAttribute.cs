using System;

namespace Umbraco.Cms.Core.Configuration.Models
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UmbracoOptionsAttribute : Attribute
    {
        public string ConfigurationKey { get; }
        public bool BindNonPublicProperties { get; set; }

        public UmbracoOptionsAttribute(string configurationKey)
        {
            ConfigurationKey = configurationKey;
        }
    }
}
