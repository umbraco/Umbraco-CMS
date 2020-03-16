using Microsoft.Extensions.Configuration;
using Umbraco.Configuration.Models;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;
using CoreDebugSettings = Umbraco.Configuration.Models.CoreDebugSettings;

namespace Umbraco.Configuration
{
    public class AspNetCoreConfigsFactory : IConfigsFactory
    {
        private readonly IConfiguration _configuration;

        public AspNetCoreConfigsFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Configs Create()
        {
            var configs = new Configs();

            configs.Add<ITourSettings>(() => new TourSettings(_configuration));
            configs.Add<ICoreDebugSettings>(() => new CoreDebugSettings(_configuration));
            configs.Add<IRequestHandlerSettings>(() => new RequestHandlerSettings(_configuration));
            configs.Add<ISecuritySettings>(() => new SecuritySettings(_configuration));
            configs.Add<IUserPasswordConfiguration>(() => new UserPasswordConfigurationSettings(_configuration));
            configs.Add<IMemberPasswordConfiguration>(() => new MemberPasswordConfigurationSettings(_configuration));
            configs.Add<IKeepAliveSettings>(() => new KeepAliveSettings(_configuration));
            configs.Add<IContentSettings>(() => new ContentSettings(_configuration));
            configs.Add<IHealthChecksSettings>(() => new HealthChecksSettingsSettings(_configuration));
            configs.Add<ILoggingSettings>(() => new LoggingSettings(_configuration));
            configs.Add<IExceptionFilterSettings>(() => new ExceptionFilterSettings(_configuration));
            configs.Add<IActiveDirectorySettings>(() => new ActiveDirectorySettings(_configuration));
            configs.Add<IRuntimeSettings>(() => new RuntimeSettings(_configuration));
            configs.Add<ITypeFinderSettings>(() => new TypeFinderSettings(_configuration));
            configs.Add<INuCacheSettings>(() => new NuCacheSettings(_configuration));
            configs.Add<IWebRoutingSettings>(() => new WebRoutingSettings(_configuration));
            configs.Add<IIndexCreatorSettings>(() => new IndexCreatorSettings(_configuration));
            configs.Add<IModelsBuilderConfig>(() => new ModelsBuilderConfig(_configuration));

            // configs.Add<IGlobalSettings>(() => GlobalSettings);
            // configs.Add<IConnectionStrings>(() => ConnectionStrings);
            // configs.Add<IHostingSettings>(() => HostingSettings);
            // configs.Add<IMachineKeyConfig>(() => MachineKeyConfig);




            return configs;
        }
    }
}
