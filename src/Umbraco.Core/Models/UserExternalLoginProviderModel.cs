namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents information about an external login provider and its association with a user.
/// </summary>
public class UserExternalLoginProviderModel
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserExternalLoginProviderModel" /> class.
    /// </summary>
    /// <param name="providerSchemeName">The authentication scheme name of the provider.</param>
    /// <param name="isLinkedOnUser">Indicates whether the provider is linked to the user.</param>
    /// <param name="hasManualLinkingEnabled">Indicates whether manual linking is enabled for this provider.</param>
    /// <param name="providerKey">The provider-specific key for the user, if linked.</param>
    public UserExternalLoginProviderModel(string providerSchemeName, bool isLinkedOnUser, bool hasManualLinkingEnabled, string? providerKey)
    {
        ProviderSchemeName = providerSchemeName;
        IsLinkedOnUser = isLinkedOnUser;
        HasManualLinkingEnabled = hasManualLinkingEnabled;
        ProviderKey = providerKey;
    }

    /// <summary>
    ///     Gets the authentication scheme name of the external login provider.
    /// </summary>
    public string ProviderSchemeName { get; }

    /// <summary>
    ///     Gets or sets the provider-specific key identifying the user with this provider.
    /// </summary>
    /// <value>
    ///     The provider key, or <c>null</c> if the user is not linked to this provider.
    /// </value>
    public string? ProviderKey { get; set; }

    /// <summary>
    ///     Gets a value indicating whether this external login provider is linked to the user.
    /// </summary>
    public bool IsLinkedOnUser { get; }

    /// <summary>
    ///     Gets a value indicating whether manual linking is enabled for this provider.
    /// </summary>
    /// <remarks>
    ///     When enabled, users can manually link/unlink their account with this provider.
    /// </remarks>
    public bool HasManualLinkingEnabled { get; }
}
