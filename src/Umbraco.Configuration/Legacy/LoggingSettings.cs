using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Implementations
{
    internal class LoggingSettings : ConfigurationManagerConfigBase, ILoggingSettings
    {
        public int MaxLogAge => UmbracoSettingsSection.Logging.MaxLogAge;
    }
}
