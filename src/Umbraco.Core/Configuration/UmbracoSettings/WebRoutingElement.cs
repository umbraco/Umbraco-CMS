using System;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class WebRoutingElement : ConfigurationElement, IWebRoutingSection
    {
        [ConfigurationProperty("trySkipIisCustomErrors", DefaultValue = "false")]
        public bool TrySkipIisCustomErrors
        {
            get { return (bool) base["trySkipIisCustomErrors"]; }
        }

        [ConfigurationProperty("internalRedirectPreservesTemplate", DefaultValue = "false")]
        public bool InternalRedirectPreservesTemplate
        {
            get { return (bool) base["internalRedirectPreservesTemplate"]; }
        }

        [ConfigurationProperty("disableAlternativeTemplates", DefaultValue = "false")]
        public bool DisableAlternativeTemplates
        {
            get { return (bool) base["disableAlternativeTemplates"]; }
        }

        [ConfigurationProperty("validateAlternativeTemplates", DefaultValue = "false")]
        public bool ValidateAlternativeTemplates
        {
            get { return (bool)base["validateAlternativeTemplates"]; }
        }

        [ConfigurationProperty("disableFindContentByIdPath", DefaultValue = "false")]
        public bool DisableFindContentByIdPath
        {
            get { return (bool) base["disableFindContentByIdPath"]; }
        }

        [ConfigurationProperty("disableRedirectUrlTracking", DefaultValue = "false")]
        public bool DisableRedirectUrlTracking
        {
            get { return (bool) base["disableRedirectUrlTracking"]; }
        }

        [ConfigurationProperty("urlProviderMode", DefaultValue = "AutoLegacy")]
        public string UrlProviderMode
        {
            get { return (string) base["urlProviderMode"]; }
        }

        [ConfigurationProperty("umbracoApplicationUrl", DefaultValue = null)]
        public string UmbracoApplicationUrl
        {
            get { return (string)base["umbracoApplicationUrl"]; }
        }
    }
}