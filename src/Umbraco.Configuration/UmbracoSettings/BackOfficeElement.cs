using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public class BackOfficeElement : UmbracoConfigurationElement, IBackOfficeSection
    {
        [ConfigurationProperty("tours")]
        internal TourConfigElement Tours => (TourConfigElement)this["tours"];

        ITourSection IBackOfficeSection.Tours => Tours;
    }
}
