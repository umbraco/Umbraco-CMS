using System.Configuration;
using Umbraco.Configuration;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;

namespace Umbraco.Core.Configuration
{
    public class ConfigsFactory : IConfigsFactory
    {
        public IHostingSettings HostingSettings { get; } = new HostingSettings();

        public ICoreDebug CoreDebug { get; } = new CoreDebug();
        public IMachineKeyConfig MachineKeyConfig { get; } = new MachineKeyConfig();
        public IIndexCreatorSettings IndexCreatorSettings { get; } = new IndexCreatorSettings();
        public INuCacheSettings NuCacheSettings { get; } = new NuCacheSettings();
        public ITypeFinderSettings TypeFinderSettings { get; } = new TypeFinderSettings();
        public IRuntimeSettings RuntimeSettings { get; } = new RuntimeSettings();
        public IActiveDirectorySettings ActiveDirectorySettings { get; } = new ActiveDirectorySettings();
        public IExceptionFilterSettings ExceptionFilterSettings { get; } = new ExceptionFilterSettings();

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
            configs.Add(() => MachineKeyConfig);
            configs.Add<IConnectionStrings>(() => new ConnectionStrings(ioHelper));
            configs.Add<IModelsBuilderConfig>(() => new ModelsBuilderConfig(ioHelper));


            configs.Add<IIndexCreatorSettings>(() => IndexCreatorSettings);
            configs.Add<INuCacheSettings>(() => NuCacheSettings);
            configs.Add<ITypeFinderSettings>(() => TypeFinderSettings);
            configs.Add<IRuntimeSettings>(() => RuntimeSettings);
            configs.Add<IActiveDirectorySettings>(() => ActiveDirectorySettings);
            configs.Add<IExceptionFilterSettings>(() => ExceptionFilterSettings);

            configs.AddCoreConfigs(ioHelper);
            return configs;
        }
    }
}
