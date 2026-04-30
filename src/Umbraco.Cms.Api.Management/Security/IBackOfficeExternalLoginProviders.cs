namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     Service to return <see cref="BackOfficeExternalLoginProvider" /> instances
/// </summary>
public interface IBackOfficeExternalLoginProviders
{
    /// <summary>
    ///     Get the <see cref="BackOfficeExternalLoginProvider" /> for the specified scheme
    /// </summary>
    /// <param name="authenticationType">The authentication scheme name.</param>
    /// <returns>The external login provider scheme, or null if not found.</returns>
    Task<BackOfficeExternaLoginProviderScheme?> GetAsync(string authenticationType);

    /// <summary>
    ///     Get all registered <see cref="BackOfficeExternalLoginProvider" />
    /// </summary>
    /// <returns>All registered back office external login provider schemes.</returns>
    Task<IEnumerable<BackOfficeExternaLoginProviderScheme>> GetBackOfficeProvidersAsync();

    /// <summary>
    ///     Returns true if there is any external provider that has the Deny Local Login option configured
    /// </summary>
    /// <returns>True if any provider has deny local login configured.</returns>
    bool HasDenyLocalLogin();

    /// <summary>
    ///     Used during startup to see if the configured external login providers is different from the persisted information.
    ///     If they are different, this will invalidate backoffice sessions and clear external logins for removed providers
    ///     if the external login provider setup has changed.
    /// </summary>
    void InvalidateSessionsIfExternalLoginProvidersChanged() { }
}
