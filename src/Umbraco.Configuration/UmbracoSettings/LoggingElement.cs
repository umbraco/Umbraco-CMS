using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class LoggingElement : UmbracoConfigurationElement, ILoggingSettings
    {

        [ConfigurationProperty("maxLogAge")]
        internal InnerTextConfigurationElement<int> MaxLogAge => GetOptionalTextElement("maxLogAge", -1);

        int ILoggingSettings.MaxLogAge => MaxLogAge;
    }
}
