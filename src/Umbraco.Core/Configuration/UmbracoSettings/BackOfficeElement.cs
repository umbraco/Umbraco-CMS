using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class BackOfficeElement : UmbracoConfigurationElement, IBackOfficeSection
    {
        [ConfigurationProperty("tours")]
        internal TourConfigElement Tours
        {
            get { return (TourConfigElement)this["tours"]; }
        }

        ITourSection IBackOfficeSection.Tours
        {
            get { return Tours; }
        }
    }
}