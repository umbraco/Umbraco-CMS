using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Implementations
{
    internal class TourSettings : ConfigurationManagerConfigBase, ITourSettings
    {
        public bool EnableTours => UmbracoSettingsSection.BackOffice.Tours.EnableTours;
    }
}
