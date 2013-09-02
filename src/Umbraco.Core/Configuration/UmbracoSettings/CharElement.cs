namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class CharElement : InnerTextConfigurationElement<string>
    {
        internal string Char
        {
            get { return RawXml.Attribute("org").Value; }
        }
    }
}