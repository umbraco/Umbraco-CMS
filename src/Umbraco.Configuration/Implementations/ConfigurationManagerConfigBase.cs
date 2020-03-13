using System.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Implementations
{
    internal abstract class ConfigurationManagerConfigBase
    {
        private UmbracoSettingsSection _umbracoSettingsSection;

        public UmbracoSettingsSection UmbracoSettingsSection
        {
            get
            {
                if (_umbracoSettingsSection is null)
                {
                    _umbracoSettingsSection = ConfigurationManager.GetSection("umbracoConfiguration/settings") as UmbracoSettingsSection;
                }
                return _umbracoSettingsSection;
            }
        }
    }
}
