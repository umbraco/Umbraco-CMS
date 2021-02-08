using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// The password configuration for members
    /// </summary>
    public class MemberPasswordConfiguration : PasswordConfiguration, IMemberPasswordConfiguration
    {
        public MemberPasswordConfiguration(IMemberPasswordConfiguration configSettings)
            : base(configSettings)
        {
        }
    }
}
