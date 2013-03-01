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
		
        private bool? _trySkipIisCustomErrors;
    
        internal protected override void ResetSection()
        {
            base.ResetSection();

            _trySkipIisCustomErrors = null;
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
    }
}
