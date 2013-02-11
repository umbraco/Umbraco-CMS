using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Sync
{
    
    internal class ConfigServerRegistrar : IServerRegistrar
    {

        private List<IServerRegistration> _addresses;

        public IEnumerable<IServerRegistration> Registrations
        {
            get
            {
                if (_addresses == null)
                {
                    _addresses = new List<IServerRegistration>();
                    var nodes = UmbracoSettings.DistributionServers.SelectNodes("./server");
                    foreach (XmlNode n in nodes)
                     {
                         _addresses.Add(new ConfigServerRegistration(n));
                     }
                }
                return _addresses;
            }
        }
    }

    internal interface IServerRegistration
    {
        string ServerAddress { get; }
    }

    internal class ConfigServerRegistration : IServerRegistration
    {

        public ConfigServerRegistration(XmlNode n)
        {
            var protocol = GlobalSettings.UseSSL ? "https" : "http";
            if (n.Attributes.GetNamedItem("forceProtocol") != null && !string.IsNullOrEmpty(n.Attributes.GetNamedItem("forceProtocol").Value))
                protocol = n.Attributes.GetNamedItem("forceProtocol").Value;
            var domain = XmlHelper.GetNodeValue(n);
            if (n.Attributes.GetNamedItem("forcePortnumber") != null && !string.IsNullOrEmpty(n.Attributes.GetNamedItem("forcePortnumber").Value))
                domain += string.Format(":{0}", n.Attributes.GetNamedItem("forcePortnumber").Value);
            ServerAddress = string.Format("{0}://{1}{2}", protocol, domain);
        }

        public string ServerAddress { get; private set; }
        
    }

    internal interface IServerRegistrar
    {
        IEnumerable<IServerRegistration> Registrations { get; } 
    }

    internal class ServerRegistrarResolver : SingleObjectResolverBase<ServerRegistrarResolver, IServerRegistrar>
    {

        internal ServerRegistrarResolver(IServerRegistrar factory)
			: base(factory)
		{
		}

        public IServerRegistrar Registrar
        {
            get { return Value; }
        }

    }
}
