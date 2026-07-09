namespace Umbraco.Cms.Core.Configuration;

/// <summary>
///     The password configuration for members.
/// </summary>
public class MemberPasswordConfiguration : PasswordConfiguration, IMemberPasswordConfiguration
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberPasswordConfiguration" /> class.
    /// </summary>
    /// <param name="configSettings">The member password configuration settings.</param>
    public MemberPasswordConfiguration(IMemberPasswordConfiguration configSettings)
        : base(configSettings)
    {
    }
}
