namespace Umbraco.Cms.Core.Configuration;

/// <summary>
///     Abstract base class for password configuration.
/// </summary>
public abstract class PasswordConfiguration : IPasswordConfiguration
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PasswordConfiguration" /> class.
    /// </summary>
    /// <param name="configSettings">The password configuration settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configSettings" /> is null.</exception>
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

    /// <inheritdoc />
    public int RequiredLength { get; }

    /// <inheritdoc />
    public bool RequireNonLetterOrDigit { get; }

    /// <inheritdoc />
    public bool RequireDigit { get; }

    /// <inheritdoc />
    public bool RequireLowercase { get; }

    /// <inheritdoc />
    public bool RequireUppercase { get; }

    /// <inheritdoc />
    public string HashAlgorithmType { get; }

    /// <inheritdoc />
    public int MaxFailedAccessAttemptsBeforeLockout { get; }
}
