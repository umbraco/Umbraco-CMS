namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class NotDynamicXmlDocumentElement : InnerTextConfigurationElement<string>, INotDynamicXmlDocument
    {
        public string Element
        {
            get { return Value; }
        }
    }
}