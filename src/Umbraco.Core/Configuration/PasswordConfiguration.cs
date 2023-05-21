namespace Umbraco.Cms.Core.Configuration;

public abstract class PasswordConfiguration : IPasswordConfiguration
{
    protected PasswordConfiguration(IPasswordConfiguration configSettings)
    {
        if (configSettings == null)
        {
            throw new ArgumentNullException(nameof(configSettings));
        }

        RequiredLength = configSettings.RequiredLength;
        RequireNonLetterOrDigit = configSettings.RequireNonLetterOrDigit;
        RequireDigit = configSettings.RequireDigit;
        RequireLowercase = configSettings.RequireLowercase;
        RequireUppercase = configSettings.RequireUppercase;
        HashAlgorithmType = configSettings.HashAlgorithmType;
        MaxFailedAccessAttemptsBeforeLockout = configSettings.MaxFailedAccessAttemptsBeforeLockout;
    }

    public int RequiredLength { get; }

    public bool RequireNonLetterOrDigit { get; }

    public bool RequireDigit { get; }

    public bool RequireLowercase { get; }

    public bool RequireUppercase { get; }

    public string HashAlgorithmType { get; }

    public int MaxFailedAccessAttemptsBeforeLockout { get; }
}
