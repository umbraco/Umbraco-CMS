namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

/// <summary>
/// Represents information about an external login provider associated with a user.
/// </summary>
public class UserExternalLoginProviderResponseModel
{
    /// <summary>
    /// Gets or sets the name of the external login provider scheme.
    /// </summary>
    public required string ProviderSchemeName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the external login provider is linked to the user.
    /// </summary>
    public bool IsLinkedOnUser { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether manual linking is enabled for the external login provider.
    /// </summary>
    public bool HasManualLinkingEnabled { get; set; }

    /// <summary>
    /// Gets or sets the unique key provided by the external authentication provider for this user.
    /// </summary>
    public string? ProviderKey { get; set; }
}
