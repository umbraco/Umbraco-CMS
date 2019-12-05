using System.Configuration;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;

namespace Umbraco.Core.Configuration
{
    public class ConfigsFactory : IConfigsFactory
    {

        public ConfigsFactory()
        {
        }

        public IHostingSettings HostingSettings { get; } = new HostingSettings();

        public Configs Create(IIOHelper ioHelper)
        {
            var configs =  new Configs(section => ConfigurationManager.GetSection(section));
            configs.Add<IGlobalSettings>(() => new GlobalSettings(ioHelper));
            configs.Add<IHostingSettings>(() => HostingSettings);

            configs.Add<IUmbracoSettingsSection>("umbracoConfiguration/settings");
            configs.Add<IHealthChecks>("umbracoConfiguration/HealthChecks");

            configs.Add<IUserPasswordConfiguration>(() => new DefaultPasswordConfig());
            configs.Add<IMemberPasswordConfiguration>(() => new DefaultPasswordConfig());
            configs.Add<ICoreDebug>(() => new CoreDebug());
            configs.Add<IConnectionStrings>(() => new ConnectionStrings());
            configs.AddCoreConfigs(ioHelper);
            return configs;
        }
    }

    // Default/static user password configs
    // TODO: Make this configurable somewhere - we've removed membership providers for users, so could be a section in the umbracosettings.config file?
    // keeping in mind that we will also be removing the members membership provider so there will be 2x the same/similar configuration.
    // TODO: Currently it doesn't actually seem possible to replace any sub-configuration unless totally replacing the IConfigsFactory??
    internal class DefaultPasswordConfig : IUserPasswordConfiguration, IMemberPasswordConfiguration
    {
        public int RequiredLength => 12;
        public bool RequireNonLetterOrDigit => false;
        public bool RequireDigit => false;
        public bool RequireLowercase => false;
        public bool RequireUppercase => false;
        public bool UseLegacyEncoding => false;
        public string HashAlgorithmType => "HMACSHA256";
        public int MaxFailedAccessAttemptsBeforeLockout => 5;
    }
}
