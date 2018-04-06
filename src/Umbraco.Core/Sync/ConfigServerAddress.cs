using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Provides the address of a server based on the Xml configuration.
    /// </summary>
    internal class ConfigServerAddress : IServerAddress
    {
        public ConfigServerAddress(IServer n, IGlobalSettings globalSettings)
        {
            var webServicesUrl = IOHelper.ResolveUrl(SystemDirectories.WebServices);

            var protocol = globalSettings.UseHttps ? "https" : "http";
            if (n.ForceProtocol.IsNullOrWhiteSpace() == false)
                protocol = n.ForceProtocol;
            var domain = n.ServerAddress;
            if (n.ForcePortnumber.IsNullOrWhiteSpace() == false)
                domain += $":{n.ForcePortnumber}";
            ServerAddress = $"{protocol}://{domain}{webServicesUrl}/cacheRefresher.asmx";
        }

        public string ServerAddress { get; private set; }

        public override string ToString()
        {
            return ServerAddress;
        }
    }
}
