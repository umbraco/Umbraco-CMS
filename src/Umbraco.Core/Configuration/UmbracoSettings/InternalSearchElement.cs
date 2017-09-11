using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class InternalSearchElement : UmbracoConfigurationElement, IInternalSearchFieldsToSearchSection
    {
        [ConfigurationProperty("contentSearchFields")]
        internal CommaDelimitedConfigurationElement ContentSearchFields
        {
            get
            {
                return GetOptionalDelimitedElement("contentSearchFields", Enumerable.Empty<string>().ToArray());
                
            }
        }

        [ConfigurationProperty("mediaSearchFields")]
        internal CommaDelimitedConfigurationElement MediaSearchFields
        {
            get
            {
                return GetOptionalDelimitedElement("mediaSearchFields", Enumerable.Empty<string>().ToArray());

            }
        }

        IEnumerable<string> IInternalSearchFieldsToSearchSection.ContentSearchFields
        {
            get { return ContentSearchFields; }
        }

        IEnumerable<string> IInternalSearchFieldsToSearchSection.MediaSearchFields
        {
            get { return MediaSearchFields; }
        }
    }
}