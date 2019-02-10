﻿using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class LoggingElement : UmbracoConfigurationElement, ILoggingSection
    {

        [ConfigurationProperty("maxLogAge")]
        internal InnerTextConfigurationElement<int> MaxLogAge => GetOptionalTextElement("maxLogAge", -1);

        int ILoggingSection.MaxLogAge => MaxLogAge;
    }
}
