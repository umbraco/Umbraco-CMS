namespace Umbraco.Cms.Core.Configuration;

/// <summary>
///     The password configuration for back office users.
/// </summary>
public class UserPasswordConfiguration : PasswordConfiguration, IUserPasswordConfiguration
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserPasswordConfiguration" /> class.
    /// </summary>
    /// <param name="configSettings">The user password configuration settings.</param>
    public UserPasswordConfiguration(IUserPasswordConfiguration configSettings)
        : base(configSettings)
    {
    }
}
