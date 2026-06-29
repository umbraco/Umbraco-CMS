using Microsoft.AspNetCore.Authentication;
using Umbraco.Cms.Core.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Security;

/// <inheritdoc />
public class BackOfficeExternalLoginProviders : IBackOfficeExternalLoginProviders, ILocalLoginSettingProvider
{
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
    private readonly Dictionary<string, BackOfficeExternalLoginProvider> _externalLogins;
    private readonly IKeyValueService _keyValueService;
    private readonly IExternalLoginWithKeyService _externalLoginWithKeyService;
    private readonly ILogger<BackOfficeExternalLoginProviders> _logger;

    private const string ExternalLoginProvidersKey = "Umbraco.Cms.Web.BackOffice.Security.BackOfficeExternalLoginProviders";

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Security.BackOfficeExternalLoginProviders"/> class.
    /// </summary>
    /// <param name="externalLogins">A collection of available external login providers for the backoffice.</param>
    /// <param name="authenticationSchemeProvider">Provides access to authentication schemes used for external logins.</param>
    /// <param name="keyValueService">Service for storing and retrieving key-value pairs related to authentication.</param>
    /// <param name="externalLoginWithKeyService">Service for handling external logins that require a key.</param>
    /// <param name="logger">The logger used for logging information and errors related to external login providers.</param>
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
        var currentExternalLoginProvidersValue = string.Join("|", _externalLogins.Keys.OrderBy(key => key));

        if ((previousExternalLoginProvidersValue ?? string.Empty) != currentExternalLoginProvidersValue)
        {
            _logger.LogWarning(
                "The configured external login providers have changed. Existing backoffice sessions using the removed providers will be invalidated and external login data removed.");

            _externalLoginWithKeyService.PurgeLoginsForRemovedProviders(_externalLogins.Keys);

            _keyValueService.SetValue(ExternalLoginProvidersKey, currentExternalLoginProvidersValue);
        }
        else if (previousExternalLoginProvidersValue is null)
        {
            _keyValueService.SetValue(ExternalLoginProvidersKey, string.Empty);
        }
    }
}
