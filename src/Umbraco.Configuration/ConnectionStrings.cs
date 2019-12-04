using System;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.IO;

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
