using Microsoft.AspNetCore.Authentication;

namespace Umbraco.Cms.Web.BackOffice.Security;

/// <inheritdoc />
public class BackOfficeExternalLoginProviders : IBackOfficeExternalLoginProviders
{
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
    private readonly Dictionary<string, BackOfficeExternalLoginProvider> _externalLogins;

    public BackOfficeExternalLoginProviders(
        IEnumerable<BackOfficeExternalLoginProvider> externalLogins,
        IAuthenticationSchemeProvider authenticationSchemeProvider)
    {
        _externalLogins = externalLogins.ToDictionary(x => x.AuthenticationType);
        _authenticationSchemeProvider = authenticationSchemeProvider;
    }

    /// <inheritdoc />
    public async Task<BackOfficeExternaLoginProviderScheme?> GetAsync(string authenticationType)
    {
        if (!_externalLogins.TryGetValue(authenticationType, out BackOfficeExternalLoginProvider? provider))
        {
            return null;
        }

        // get the associated scheme
        AuthenticationScheme? associatedScheme =
            await _authenticationSchemeProvider.GetSchemeAsync(provider.AuthenticationType);

        if (associatedScheme == null)
        {
            throw new InvalidOperationException(
                "No authentication scheme registered for " + provider.AuthenticationType);
        }

        return new BackOfficeExternaLoginProviderScheme(provider, associatedScheme);
    }

    /// <inheritdoc />
    public string? GetAutoLoginProvider()
    {
        var found = _externalLogins.Values.Where(x => x.Options.AutoRedirectLoginToExternalProvider).ToList();
        return found.Count > 0 ? found[0].AuthenticationType : null;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BackOfficeExternaLoginProviderScheme>> GetBackOfficeProvidersAsync()
    {
        var providersWithSchemes = new List<BackOfficeExternaLoginProviderScheme>();
        foreach (BackOfficeExternalLoginProvider login in _externalLogins.Values)
        {
            // get the associated scheme
            AuthenticationScheme? associatedScheme =
                await _authenticationSchemeProvider.GetSchemeAsync(login.AuthenticationType);

            providersWithSchemes.Add(new BackOfficeExternaLoginProviderScheme(login, associatedScheme));
        }

        return providersWithSchemes;
    }

    /// <inheritdoc />
    public bool HasDenyLocalLogin()
    {
        var found = _externalLogins.Values.Where(x => x.Options.DenyLocalLogin).ToList();
        return found.Count > 0;
    }
}
