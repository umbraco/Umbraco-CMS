namespace Umbraco.Cms.Core.Configuration.UmbracoSettings;

/// <summary>
///     Defines the password configuration section settings.
/// </summary>
public interface IPasswordConfigurationSection : IUmbracoConfigurationSection
{
    /// <summary>
    ///     Gets the minimum required length for passwords.
    /// </summary>
    int RequiredLength { get; }

    /// <summary>
    ///     Gets a value indicating whether passwords must contain at least one non-letter or digit character.
    /// </summary>
    bool RequireNonLetterOrDigit { get; }

    /// <summary>
    ///     Gets a value indicating whether passwords must contain at least one digit.
    /// </summary>
    bool RequireDigit { get; }

    /// <summary>
    ///     Gets a value indicating whether passwords must contain at least one lowercase character.
    /// </summary>
    bool RequireLowercase { get; }

    /// <summary>
    ///     Gets a value indicating whether passwords must contain at least one uppercase character.
    /// </summary>
    bool RequireUppercase { get; }

    /// <summary>
    ///     Gets a value indicating whether to use legacy password encoding.
    /// </summary>
    bool UseLegacyEncoding { get; }

    /// <summary>
    ///     Gets the hash algorithm type used for password hashing.
    /// </summary>
    string HashAlgorithmType { get; }

    /// <summary>
    ///     Gets the maximum number of failed access attempts before the account is locked out.
    /// </summary>
    int MaxFailedAccessAttemptsBeforeLockout { get; }
}
