using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.BackOffice.Security;

/// <inheritdoc />
public class BackOfficeExternalLoginProviders : IBackOfficeExternalLoginProviders
{
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
    private readonly Dictionary<string, BackOfficeExternalLoginProvider> _externalLogins;
    private readonly IKeyValueService _keyValueService;
    private readonly IExternalLoginWithKeyService _externalLoginWithKeyService;
    private readonly ILogger<BackOfficeExternalLoginProviders> _logger;

    private const string ExternalLoginProvidersKey = "Umbraco.Cms.Web.BackOffice.Security.BackOfficeExternalLoginProviders";

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 17.")]
    public BackOfficeExternalLoginProviders(
        IEnumerable<BackOfficeExternalLoginProvider> externalLogins,
        IAuthenticationSchemeProvider authenticationSchemeProvider)
        : this(
              externalLogins,
              authenticationSchemeProvider,
              StaticServiceProvider.Instance.GetRequiredService<IKeyValueService>(),
              StaticServiceProvider.Instance.GetRequiredService<IExternalLoginWithKeyService>(),
              StaticServiceProvider.Instance.GetRequiredService<ILogger<BackOfficeExternalLoginProviders>>())
    {
    }

    public BackOfficeExternalLoginProviders(
        IEnumerable<BackOfficeExternalLoginProvider> externalLogins,
        IAuthenticationSchemeProvider authenticationSchemeProvider,
        IKeyValueService keyValueService,
        IExternalLoginWithKeyService externalLoginWithKeyService,
        ILogger<BackOfficeExternalLoginProviders> logger)
    {
        _externalLogins = externalLogins.ToDictionary(x => x.AuthenticationType);
        _authenticationSchemeProvider = authenticationSchemeProvider;
        _keyValueService = keyValueService;
        _externalLoginWithKeyService = externalLoginWithKeyService;
        _logger = logger;
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

    /// <inheritdoc />
    public void InvalidateSessionsIfExternalLoginProvidersChanged()
    {
        var previousExternalLoginProvidersValue = _keyValueService.GetValue(ExternalLoginProvidersKey);
        var currentExternalLoginProvidersValue = string.Join("|", _externalLogins.Keys);

        if ((previousExternalLoginProvidersValue ?? string.Empty) != currentExternalLoginProvidersValue)
        {
            _logger.LogWarning(
                "The configured external login providers have changed. All existing backoffice tokens will be invalidated");

            _externalLoginWithKeyService.DeleteUserLoginsForRemovedProviders(_externalLogins.Keys);

            _keyValueService.SetValue(ExternalLoginProvidersKey, currentExternalLoginProvidersValue);
        }
        else if (previousExternalLoginProvidersValue is null)
        {
            _keyValueService.SetValue(ExternalLoginProvidersKey, string.Empty);
        }
    }
}
