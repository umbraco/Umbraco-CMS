using System.ComponentModel;
using System.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.Configuration
{
    /// <summary>
    /// The Web.Routing settings section.
    /// </summary>
    [ConfigurationKey("umbraco/web.routing")]
    internal class WebRouting : UmbracoConfigurationSection
    {
        private const string KeyTrySkipIisCustomErrors = "trySkipIisCustomErrors";
        private const string KeyUrlProviderMode = "urlProviderMode";
        private const string KeyInternalRedirectPreservesTemplate = "internalRedirectPreservesTemplate";
		
        private bool? _trySkipIisCustomErrors;
        private Routing.UrlProviderMode? _urlProviderMode;
        private bool? _internalRedirectPreservesTemplate;
    
        internal protected override void ResetSection()
        {
            base.ResetSection();

            _trySkipIisCustomErrors = null;
			_urlProviderMode = null;
            _internalRedirectPreservesTemplate = null;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to try to skip IIS custom errors.
        /// </summary>
        [ConfigurationProperty(KeyTrySkipIisCustomErrors, DefaultValue = false, IsRequired = false)]
        public bool TrySkipIisCustomErrors
        {
            get 
            {
                return _trySkipIisCustomErrors ?? (IsPresent 
                    ? (bool)this[KeyTrySkipIisCustomErrors] 
                    : UmbracoSettings.TrySkipIisCustomErrors);
            }
            internal set { _trySkipIisCustomErrors = value; }
        }

        /// <summary>
        /// Gets or sets the url provider mode.
        /// </summary>
        /// <remarks>If the section is present then default is Auto, else default is AutoLegacy.</remarks>
        [ConfigurationProperty(KeyUrlProviderMode, DefaultValue = Routing.UrlProviderMode.Auto, IsRequired = false)]
        [TypeConverter(typeof(CaseInsensitiveEnumConfigConverter<Routing.UrlProviderMode>))]
        public Routing.UrlProviderMode UrlProviderMode
        {
            get
            {
                return _urlProviderMode ?? (IsPresent
                    ? (Routing.UrlProviderMode)this[KeyUrlProviderMode]
                    : Routing.UrlProviderMode.AutoLegacy);
            }
            internal set { _urlProviderMode = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether internal redirect preserves the template.
        /// </summary>
        [ConfigurationProperty(KeyInternalRedirectPreservesTemplate, DefaultValue = false, IsRequired = false)]
        public bool InternalRedirectPreservesTemplate
        {
            get
            {
                return _internalRedirectPreservesTemplate ?? (IsPresent
                    ? (bool)this[KeyInternalRedirectPreservesTemplate]
                    : UmbracoSettings.InternalRedirectPreservesTemplate);
            }
            internal set { _internalRedirectPreservesTemplate = value; }
        }
    }
}
