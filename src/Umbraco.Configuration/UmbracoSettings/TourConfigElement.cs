using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class TourConfigElement : UmbracoConfigurationElement, ITourSection
    {
        //disabled by default so that upgraders don't get it enabled by default
        // TODO: we probably just want to disable the initial one from automatically loading ?
        [ConfigurationProperty("enable", DefaultValue = false)]
        public bool EnableTours => (bool)this["enable"];

        // TODO: We could have additional filters, etc... defined here
    }
}
