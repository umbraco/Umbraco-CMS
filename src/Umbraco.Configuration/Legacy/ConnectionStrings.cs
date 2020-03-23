using System.Configuration;

namespace Umbraco.Core.Configuration
{
    public class ConnectionStrings : IConnectionStrings
    {
        public ConfigConnectionString this[string key]
        {
            get
            {
                var settings = ConfigurationManager.ConnectionStrings[key];
                if (settings == null) return null;
                return new ConfigConnectionString(settings.ConnectionString, settings.ProviderName, settings.Name);
            }
        }
    }
}
