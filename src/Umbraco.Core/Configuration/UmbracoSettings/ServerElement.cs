namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ServerElement : InnerTextConfigurationElement<string>, IServer
    {
        public string ForcePortnumber
        {
            get
            {
                return RawXml.Attribute("forcePortnumber") == null
                           ? null
                           : RawXml.Attribute("forcePortnumber").Value;
            }
        }

        public string ForceProtocol
        {
            get
            {
                return RawXml.Attribute("forceProtocol") == null
                           ? null
                           : RawXml.Attribute("forceProtocol").Value;
            }
        }  

        string IServer.ServerAddress
        {
            get { return Value; }
        }

        public string AppId
        {
            get
            {
                return RawXml.Attribute("appId") == null
                           ? null
                           : RawXml.Attribute("appId").Value;
            }
        }
        public string ServerName
        {
            get
            {
                return RawXml.Attribute("serverName") == null
                           ? null
                           : RawXml.Attribute("serverName").Value;
            }
        }
    }
}