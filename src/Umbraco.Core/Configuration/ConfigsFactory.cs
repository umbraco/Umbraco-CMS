using System.Configuration;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;

namespace Umbraco.Core.Configuration
{
    public class ConfigsFactory : IConfigsFactory
    {
        private readonly IIOHelper _ioHelper;
        private readonly ISystemDirectories _systemDirectories;

        public ConfigsFactory(IIOHelper ioHelper, ISystemDirectories systemDirectories)
        {
            _ioHelper = ioHelper;
            _systemDirectories = systemDirectories;
        }

        public Configs Create() {
            var configs =  new Configs(section => ConfigurationManager.GetSection(section));
            configs.Add<IGlobalSettings>(() => new GlobalSettings(_ioHelper, _systemDirectories));
            configs.Add<IUmbracoSettingsSection>("umbracoConfiguration/settings");
            configs.Add<IHealthChecks>("umbracoConfiguration/HealthChecks");

            configs.Add(() => new CoreDebug());
            configs.AddCoreConfigs(_ioHelper, _systemDirectories);
            return configs;
        }
    }
}
