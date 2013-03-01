using System.ComponentModel;
using System.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.Configuration
{
    /// <summary>
    /// The Web.Routing settings section.
    /// </summary>
    [ConfigurationKey("web.routing", ConfigurationKeyType.Umbraco)]
    internal class WebRouting : UmbracoConfigurationSection
    {
        private const string KeyTrySkipIisCustomErrors = "trySkipIisCustomErrors";
        private const string KeyUrlProviderMode = "urlProviderMode";
		
        private bool? _trySkipIisCustomErrors;
        private Routing.UrlProviderMode? _urlProviderMode;
    
        internal protected override void ResetSection()
        {
            base.ResetSection();

            _trySkipIisCustomErrors = null;
			_urlProviderMode = null;
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
        [ConfigurationProperty(KeyUrlProviderMode, DefaultValue = Routing.UrlProviderMode.AutoLegacy, IsRequired = false)]
        [TypeConverter(typeof(CaseInsensitiveEnumConfigConverter<Routing.UrlProviderMode>))]
        public Routing.UrlProviderMode UrlProviderMode
        {
            get
            {
                return _urlProviderMode ?? (IsPresent
                    ? (Routing.UrlProviderMode)this[KeyUrlProviderMode]
                    : Routing.UrlProviderMode.Auto);
            }
            internal set { _urlProviderMode = value; }
        }
    }
}
