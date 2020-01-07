using System.Configuration;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;

namespace Umbraco.Core.Configuration
{
    public class ConfigsFactory : IConfigsFactory
    {

        public ConfigsFactory()
        {
        }

        public IHostingSettings HostingSettings { get; } = new HostingSettings();

        public ICoreDebug CoreDebug { get; } = new CoreDebug();

        public IUmbracoSettingsSection UmbracoSettings { get; }

        public Configs Create(IIOHelper ioHelper)
        {
            var configs =  new Configs(section => ConfigurationManager.GetSection(section));
            configs.Add<IGlobalSettings>(() => new GlobalSettings(ioHelper));
            configs.Add(() => HostingSettings);

            configs.Add<IUmbracoSettingsSection>("umbracoConfiguration/settings");
            configs.Add<IHealthChecks>("umbracoConfiguration/HealthChecks");

            // Password configuration is held within IUmbracoSettingsSection from umbracoConfiguration/settings but we'll add explicitly
            // so it can be independently retrieved in classes that need it.
            configs.AddPasswordConfigurations();

            configs.Add(() => CoreDebug);
            configs.Add<IConnectionStrings>(() => new ConnectionStrings());
            configs.AddCoreConfigs(ioHelper);
            return configs;
        }
    }
}
