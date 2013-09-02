namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ServerElement : InnerTextConfigurationElement<string>
    {
        internal string ForcePortnumber
        {
            get
            {
                return RawXml.Attribute("forcePortnumber") == null
                           ? null
                           : RawXml.Attribute("forcePortnumber").Value;
            }
        }

        internal string ForceProtocol
        {
            get
            {
                return RawXml.Attribute("forceProtocol") == null
                           ? null
                           : RawXml.Attribute("forceProtocol").Value;
            }
        }
    }
}