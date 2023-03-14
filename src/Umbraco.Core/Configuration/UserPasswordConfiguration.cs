namespace Umbraco.Cms.Core.Configuration;

/// <summary>
///     The password configuration for back office users
/// </summary>
public class UserPasswordConfiguration : PasswordConfiguration, IUserPasswordConfiguration
{
    public UserPasswordConfiguration(IUserPasswordConfiguration configSettings)
        : base(configSettings)
    {
    }
}
