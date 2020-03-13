using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class BackOfficeElement : UmbracoConfigurationElement, IBackOfficeSection
    {
        [ConfigurationProperty("tours")]
        internal TourConfigElement Tours => (TourConfigElement)this["tours"];

        ITourSettings IBackOfficeSection.Tours => Tours;
    }
}
