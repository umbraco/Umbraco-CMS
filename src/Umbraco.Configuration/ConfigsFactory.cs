using System.Configuration;
using Umbraco.Configuration;
using Umbraco.Configuration.Implementations;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

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
        public ITourSettings TourSettings { get; } = new TourSettings();
        public ILoggingSettings LoggingSettings { get; } = new LoggingSettings();
        public IKeepAliveSettings KeepAliveSettings { get; } = new KeepAliveSettings();
        public IWebRoutingSettings WebRoutingSettings { get; } = new WebRoutingSettings();
        public IRequestHandlerSettings RequestHandlerSettings { get; } = new RequestHandlerSettings();
        public ISecuritySettings SecuritySettings { get; } = new SecuritySettings();
        public IUserPasswordConfiguration UserPasswordConfigurationSettings { get; } = new UserPasswordConfigurationSettings();
        public IMemberPasswordConfiguration MemberPasswordConfigurationSettings { get; } = new MemberPasswordConfigurationSettings();
        public IContentSettings ContentSettings { get; } = new ContentSettings();
        public IGlobalSettings GlobalSettings { get; } = new GlobalSettings();

        public Configs Create(IIOHelper ioHelper, ILogger logger)
        {
            var configs =  new Configs(section => ConfigurationManager.GetSection(section));
            configs.Add<IGlobalSettings>(() => GlobalSettings);
            configs.Add(() => HostingSettings);

            configs.Add<IHealthChecks>("umbracoConfiguration/HealthChecks");

            configs.Add(() => CoreDebug);
            configs.Add(() => MachineKeyConfig);
            configs.Add<IConnectionStrings>(() => new ConnectionStrings(ioHelper, logger));
            configs.Add<IModelsBuilderConfig>(() => new ModelsBuilderConfig(ioHelper));


            configs.Add<IIndexCreatorSettings>(() => IndexCreatorSettings);
            configs.Add<INuCacheSettings>(() => NuCacheSettings);
            configs.Add<ITypeFinderSettings>(() => TypeFinderSettings);
            configs.Add<IRuntimeSettings>(() => RuntimeSettings);
            configs.Add<IActiveDirectorySettings>(() => ActiveDirectorySettings);
            configs.Add<IExceptionFilterSettings>(() => ExceptionFilterSettings);

            configs.Add<ITourSettings>(() => TourSettings);
            configs.Add<ILoggingSettings>(() => LoggingSettings);
            configs.Add<IKeepAliveSettings>(() => KeepAliveSettings);
            configs.Add<IWebRoutingSettings>(() => WebRoutingSettings);
            configs.Add<IRequestHandlerSettings>(() => RequestHandlerSettings);
            configs.Add<ISecuritySettings>(() => SecuritySettings);
            configs.Add<IUserPasswordConfiguration>(() => UserPasswordConfigurationSettings);
            configs.Add<IMemberPasswordConfiguration>(() => MemberPasswordConfigurationSettings);
            configs.Add<IContentSettings>(() => ContentSettings);

            configs.AddCoreConfigs();
            return configs;
        }
    }
}
