using System.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.BaseRest.Configuration
{
    // note: the name should be "BaseRest" but we keep it "BaseRestSection" for compat. reasons.

    [ConfigurationKey("BaseRestExtensions")]
    internal class BaseRestSection : UmbracoConfigurationSection
    {
        private const string KeyEnabled = "enabled";

        private bool? _enabled;

        internal protected override void ResetSection()
        {
            base.ResetSection();

            _enabled = null;
        }

        [ConfigurationProperty("", IsKey = false, IsRequired = false, IsDefaultCollection = true)]
		public ExtensionElementCollection Items
		{
			get { return (ExtensionElementCollection)base[""]; }
		}

        /// <summary>
        /// Gets or sets a value indicating whether base rest extensions are enabled.
        /// </summary>
        [ConfigurationProperty(KeyEnabled, DefaultValue = true, IsRequired = false)]
        public bool Enabled
        {
            get
            {
                return _enabled ?? (IsPresent
                    ? (bool)this[KeyEnabled]
                    : true);
            }
            internal set { _enabled = value; }
        }
    }
}
