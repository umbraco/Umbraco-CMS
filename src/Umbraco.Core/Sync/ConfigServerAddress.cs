using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// A server registration based on the legacy umbraco xml configuration in umbracoSettings
    /// </summary>
    internal class ConfigServerAddress : IServerAddress
    {

        public ConfigServerAddress(IServer n)
        {
            var webServicesUrl = IOHelper.ResolveUrl(SystemDirectories.WebServices);

            var protocol = GlobalSettings.UseSSL ? "https" : "http";
            if (n.ForceProtocol.IsNullOrWhiteSpace() == false)
                protocol = n.ForceProtocol;
            var domain = n.ServerAddress;
            if (n.ForcePortnumber.IsNullOrWhiteSpace() == false)
                domain += string.Format(":{0}", n.ForcePortnumber);
            ServerAddress = string.Format("{0}://{1}{2}/cacheRefresher.asmx", protocol, domain, webServicesUrl);
        }

        public string ServerAddress { get; private set; }

        public override string ToString()
        {
            return ServerAddress;
        }
        
    }
}