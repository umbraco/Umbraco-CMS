using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class InternalSearchElement : UmbracoConfigurationElement, IInternalSearchFieldsToSearchSection
    {
        [ConfigurationProperty("contentSearchFields")]
        internal InnerTextConfigurationElement<string> ContentSearchFields
        {
            get { return GetOptionalTextElement("contentSearchFields", string.Empty);}
        }

        string IInternalSearchFieldsToSearchSection.ContentSearchFields
        {
            get { return ContentSearchFields; }
        }
    }
}