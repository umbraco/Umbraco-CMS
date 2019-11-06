using System.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Composing
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
