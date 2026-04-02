namespace Umbraco.Cms.Api.Management.ViewModels.Security;

/// <summary>
/// Represents a response model containing the password configuration settings for the security system.
/// </summary>
public class PasswordConfigurationResponseModel
{
    /// <summary>
    /// Gets or sets the minimum length required for a password.
    /// </summary>
    public int MinimumPasswordLength { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a non-letter or digit character is required in the password.
    /// </summary>
    public bool RequireNonLetterOrDigit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a digit is required in the password.
    /// </summary>
    public bool RequireDigit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a lowercase character is required in the password.
    /// </summary>
    public bool RequireLowercase { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an uppercase letter is required in the password.
    /// </summary>
    public bool RequireUppercase { get; set; }
}
