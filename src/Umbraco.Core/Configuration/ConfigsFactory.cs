using System.Configuration;

namespace Umbraco.Core.Configuration
{
    public class ConfigsFactory : IConfigsFactory
    {
        public Configs Create() {
            var configs =  new Configs(section => ConfigurationManager.GetSection(section));
            configs.AddCoreConfigs();
            return configs;
        }
    }
}
