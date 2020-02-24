using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// The password configuration for back office users
    /// </summary>
    public class UserPasswordConfiguration : PasswordConfiguration, IUserPasswordConfiguration
    {
        public UserPasswordConfiguration(IUserPasswordConfigurationSection configSection)
            : base(configSection)
        {                
        }
    }
}
