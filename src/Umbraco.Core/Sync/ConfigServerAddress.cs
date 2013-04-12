using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// A server registration based on the legacy umbraco xml configuration in umbracoSettings
    /// </summary>
    internal class ConfigServerAddress : IServerAddress
    {

        public ConfigServerAddress(XmlNode n)
        {
            var webServicesUrl = IOHelper.ResolveUrl(SystemDirectories.WebServices);

            var protocol = GlobalSettings.UseSSL ? "https" : "http";
            if (n.Attributes.GetNamedItem("forceProtocol") != null && !string.IsNullOrEmpty(n.Attributes.GetNamedItem("forceProtocol").Value))
                protocol = n.Attributes.GetNamedItem("forceProtocol").Value;
            var domain = XmlHelper.GetNodeValue(n);
            if (n.Attributes.GetNamedItem("forcePortnumber") != null && !string.IsNullOrEmpty(n.Attributes.GetNamedItem("forcePortnumber").Value))
                domain += string.Format(":{0}", n.Attributes.GetNamedItem("forcePortnumber").Value);
            ServerAddress = string.Format("{0}://{1}{2}/cacheRefresher.asmx", protocol, domain, webServicesUrl);
        }

        public string ServerAddress { get; private set; }

        public override string ToString()
        {
            return ServerAddress;
        }
        
    }
}