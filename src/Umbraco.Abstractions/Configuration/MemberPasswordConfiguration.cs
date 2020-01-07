using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// The password configuration for back office users
    /// </summary>
    public class MemberPasswordConfiguration : PasswordConfiguration, IMemberPasswordConfiguration
    {
        public MemberPasswordConfiguration(IMemberPasswordConfigurationSection configSection)
            : base(configSection)
        {                
        }
    }
}
