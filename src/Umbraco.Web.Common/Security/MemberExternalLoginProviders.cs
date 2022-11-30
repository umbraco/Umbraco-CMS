using Microsoft.AspNetCore.Authentication;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security;

/// <inheritdoc />
public class MemberExternalLoginProviders : IMemberExternalLoginProviders
{
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
    private readonly Dictionary<string, MemberExternalLoginProvider> _externalLogins;

    public MemberExternalLoginProviders(
        IEnumerable<MemberExternalLoginProvider> externalLogins,
        IAuthenticationSchemeProvider authenticationSchemeProvider)
    {
        _externalLogins = externalLogins.ToDictionary(x => x.AuthenticationType);
        _authenticationSchemeProvider = authenticationSchemeProvider;
    }

    /// <inheritdoc />
    public async Task<MemberExternalLoginProviderScheme?> GetAsync(string authenticationType)
    {
        var schemaName =
            authenticationType.EnsureStartsWith(Core.Constants.Security.MemberExternalAuthenticationTypePrefix);

        if (!_externalLogins.TryGetValue(schemaName, out MemberExternalLoginProvider? provider))
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

        return new MemberExternalLoginProviderScheme(provider, associatedScheme);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MemberExternalLoginProviderScheme>> GetMemberProvidersAsync()
    {
        var providersWithSchemes = new List<MemberExternalLoginProviderScheme>();
        foreach (MemberExternalLoginProvider login in _externalLogins.Values)
        {
            // get the associated scheme
            AuthenticationScheme? associatedScheme =
                await _authenticationSchemeProvider.GetSchemeAsync(login.AuthenticationType);

            providersWithSchemes.Add(new MemberExternalLoginProviderScheme(login, associatedScheme));
        }

        return providersWithSchemes;
    }
}
