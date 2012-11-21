using System.Configuration;

namespace Umbraco.Core.Configuration
{
    public class ProviderFeatureSection : ConfigurationSection
    {
        private readonly ConfigurationProperty _defaultProvider = new ConfigurationProperty("defaultProvider", typeof(string), null);

        private readonly ConfigurationProperty _providers = new ConfigurationProperty("providers", typeof(ProviderSettingsCollection), null);

        private readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public ProviderFeatureSection()
        {
            _properties.Add(_providers);
            _properties.Add(_defaultProvider);
        }

        [ConfigurationProperty("defaultProvider")]
        public string DefaultProvider
        {
            get { return (string)base[_defaultProvider]; }
            set { base[_defaultProvider] = value; }
        }

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get { return (ProviderSettingsCollection)base[_providers]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }
    }
}