namespace Umbraco.Cms.Core.Configuration
{
    /// <summary>
    /// The password configuration for back office users
    /// </summary>
    public class MemberPasswordConfiguration : PasswordConfiguration, IMemberPasswordConfiguration
    {
        public MemberPasswordConfiguration(IMemberPasswordConfiguration configSettings)
            : base(configSettings)
        {
        }
    }
}
