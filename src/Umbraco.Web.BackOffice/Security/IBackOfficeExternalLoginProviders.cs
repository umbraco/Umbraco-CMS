namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Service to return <see cref="BackOfficeExternalLoginProvider" /> instances
/// </summary>
public interface IBackOfficeExternalLoginProviders
{
    /// <summary>
    ///     Get the <see cref="BackOfficeExternalLoginProvider" /> for the specified scheme
    /// </summary>
    /// <param name="authenticationType"></param>
    /// <returns></returns>
    Task<BackOfficeExternaLoginProviderScheme?> GetAsync(string authenticationType);

    /// <summary>
    ///     Get all registered <see cref="BackOfficeExternalLoginProvider" />
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<BackOfficeExternaLoginProviderScheme>> GetBackOfficeProvidersAsync();

    /// <summary>
    ///     Returns the authentication type for the last registered external login (oauth) provider that specifies an
    ///     auto-login redirect option
    /// </summary>
    /// <param name="manager"></param>
    /// <returns></returns>
    string? GetAutoLoginProvider();

    /// <summary>
    ///     Returns true if there is any external provider that has the Deny Local Login option configured
    /// </summary>
    /// <returns></returns>
    bool HasDenyLocalLogin();
}
