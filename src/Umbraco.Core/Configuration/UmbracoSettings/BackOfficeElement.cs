using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class BackOfficeElement : UmbracoConfigurationElement, IBackOfficeSection
    {
        [ConfigurationProperty("tours")]
        internal TourConfigElement Tours => (TourConfigElement)this["tours"];

        ITourSection IBackOfficeSection.Tours => Tours;

        [ConfigurationProperty("id", DefaultValue = "")]
        internal string Id => (string)base["id"];

        string IBackOfficeSection.Id => (string)base["id"];
    }
}
