using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class BackOfficeElement : UmbracoConfigurationElement, IBackOfficeSection
    {
        [ConfigurationProperty("tours")]
        internal TourConfigElement Tours => (TourConfigElement)this["tours"];

        ITourSection IBackOfficeSection.Tours => Tours;

        [ConfigurationProperty("serviceWorker")]
        internal ServiceWorkerConfigElement ServiceWorker => (ServiceWorkerConfigElement)this["serviceWorker"];
        
        IServiceWorkerSection IBackOfficeSection.ServiceWorker => ServiceWorker;
    }
}
