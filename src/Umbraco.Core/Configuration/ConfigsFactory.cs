using System.Configuration;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Configuration
{
    public class ConfigsFactory : IConfigsFactory
    {
        public Configs Create() {
            var configs =  new Configs(section => ConfigurationManager.GetSection(section));
            configs.Add<IGlobalSettings>(() => new GlobalSettings());
            configs.Add<IUmbracoSettingsSection>("umbracoConfiguration/settings");
            configs.Add<IHealthChecks>("umbracoConfiguration/HealthChecks");

            configs.Add(() => new CoreDebug());
            configs.AddCoreConfigs();
            return configs;
        }
    }
}
