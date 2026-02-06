using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Api.Management.Security;

public class BackOfficeApplicationManager : OpenIdDictApplicationManagerBase, IBackOfficeApplicationManager
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IRuntimeState _runtimeState;
    private readonly ILogger<BackOfficeApplicationManager> _logger;
    private readonly Uri? _backOfficeHost;
    private readonly string _authorizeCallbackPathName;
    private readonly string _authorizeCallbackLogoutPathName;

    [Obsolete("Use the non obsoleted constructor instead. Scheduled for removal in v19")]
    public BackOfficeApplicationManager(
        IOpenIddictApplicationManager applicationManager,
        IWebHostEnvironment webHostEnvironment,
        IOptions<SecuritySettings> securitySettings,
        IRuntimeState runtimeState)
        : base(applicationManager)
    {
        _webHostEnvironment = webHostEnvironment;
        _runtimeState = runtimeState;
        _backOfficeHost = securitySettings.Value.BackOfficeHost;
        _authorizeCallbackPathName = securitySettings.Value.AuthorizeCallbackPathName;
        _authorizeCallbackLogoutPathName = securitySettings.Value.AuthorizeCallbackLogoutPathName;
        _logger = StaticServiceProvider.Instance.GetRequiredService<ILogger<BackOfficeApplicationManager>>();
    }

    public BackOfficeApplicationManager(
        IOpenIddictApplicationManager applicationManager,
        IWebHostEnvironment webHostEnvironment,
        IOptions<SecuritySettings> securitySettings,
        IRuntimeState runtimeState,
        ILogger<BackOfficeApplicationManager> logger)
        : base(applicationManager)
    {
        _webHostEnvironment = webHostEnvironment;
        _runtimeState = runtimeState;
        _logger = logger;
        _backOfficeHost = securitySettings.Value.BackOfficeHost;
        _authorizeCallbackPathName = securitySettings.Value.AuthorizeCallbackPathName;
        _authorizeCallbackLogoutPathName = securitySettings.Value.AuthorizeCallbackLogoutPathName;
    }

    public async Task EnsureBackOfficeApplicationAsync(IEnumerable<Uri> backOfficeHosts, CancellationToken cancellationToken = default)
    {
        // Install is okay without this, because we do not need a token to install,
        // but upgrades do, so we need to execute for everything higher than or equal to upgrade.
        if (_runtimeState.Level < RuntimeLevel.Upgrade)
        {
            return;
        }

        Uri[] backOfficeHostsAsArray = backOfficeHosts as Uri[] ?? backOfficeHosts.ToArray();
        if (backOfficeHostsAsArray.Any(url => url.IsAbsoluteUri) is false)
        {
            throw new ArgumentException($"Expected absolute URLs, got: {string.Join(", ", backOfficeHostsAsArray.Select(url => url.ToString()))}", nameof(backOfficeHosts));
        }

        // A balanced environment:
        // - has 2 or more dedicated CD (Content Delivery) servers - ServerRole.Subscriber
        // - has 1 or more dedicated CM (Content Management) servers - ServerRole.SchedulingPublisher
        // The CD and CM URLs are different for the backoffice and the individual servers do not start at the same time, for example:
        // CD www.domain.com/umbraco
        // CM cms.domain.com/umbraco
        // To access the Umbraco Backoffice, it is absolutely necessary to add the address of each server to the OpenId, as they share one database!
        // Destination table: umbracoOpenIddictApplications
        // Destination Fields: RedirectUris and PostLogoutRedirectUris
        // Read saved settings from DB and add unique additional servers.
        backOfficeHostsAsArray = await MergeWithExistingBackOfficeHostsAsync(backOfficeHostsAsArray, cancellationToken);

        await CreateOrUpdate(
            BackofficeOpenIddictApplicationDescriptor(backOfficeHostsAsArray),
            cancellationToken);

        if (_webHostEnvironment.IsProduction())
        {
            await Delete(Constants.OAuthClientIds.OpenApiUi, cancellationToken);
            await Delete(Constants.OAuthClientIds.Postman, cancellationToken);
        }
        else
        {
            await CreateOrUpdate(
                DeveloperOpenIddictApplicationDescriptor(
                    "Umbraco OpenAPI access",
                    Constants.OAuthClientIds.OpenApiUi,
                    backOfficeHostsAsArray.Select(backOfficeUrl => CallbackUrlFor(backOfficeUrl, "/umbraco/openapi/oauth2-redirect.html")).ToArray()),
                cancellationToken);

            await CreateOrUpdate(
                DeveloperOpenIddictApplicationDescriptor(
                    "Umbraco Postman access",
                    Constants.OAuthClientIds.Postman,
                    [new Uri("https://oauth.pstmn.io/v1/callback"), new Uri("https://oauth.pstmn.io/v1/browser-callback")]),
                cancellationToken);
        }
    }

    public async Task EnsureBackOfficeClientCredentialsApplicationAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default)
    {
        var applicationDescriptor = new OpenIddictApplicationDescriptor
        {
            DisplayName = $"Umbraco client credentials back-office access: {clientId}",
            ClientId = clientId,
            ClientSecret = clientSecret,
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.Revocation,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials
            }
        };

        await CreateOrUpdate(applicationDescriptor, cancellationToken);
    }

    public async Task DeleteBackOfficeClientCredentialsApplicationAsync(string clientId, CancellationToken cancellationToken = default)
        => await Delete(clientId, cancellationToken);

    /// <summary>
    /// Merges new back-office hosts with existing hosts from the database.
    /// Uses OpenIddict API to read existing redirect URIs and extracts unique authorities (hosts).
    /// Handles invalid URIs gracefully by skipping them.
    /// </summary>
    /// <param name="newHosts">The new hosts to merge</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of merged unique hosts (by authority, case-insensitive)</returns>
    private async Task<Uri[]> MergeWithExistingBackOfficeHostsAsync(Uri[] newHosts, CancellationToken cancellationToken)
    {
        // Find an existing back-office application
        var application = await ApplicationManager.FindByClientIdAsync(Constants.OAuthClientIds.BackOffice, cancellationToken);
        if (application is null)
        {
            // No existing application, return new hosts as-is
            return newHosts;
        }

        // Get existing redirect URIs using OpenIddict API
        ImmutableArray<string> existingRedirectUris = await ApplicationManager.GetRedirectUrisAsync(application, cancellationToken);

        // Use HashSet for O(n) performance and automatic deduplication
        // Case-insensitive comparison for authorities (host names)
        var mergedAuthorities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Extract authorities from existing redirect URIs
        foreach (var existingUriString in existingRedirectUris)
        {
            if (TryGetAuthorityFromUriString(existingUriString, out var authority))
            {
                mergedAuthorities.Add(authority);
            }
        }

        // Add new hosts' authorities
        foreach (Uri newHost in newHosts)
        {
            if (TryGetAuthorityFromUri(newHost, out var authority))
            {
                mergedAuthorities.Add(authority);
            }
        }

        // Convert back to Uri array
        return mergedAuthorities.Select(authority => new Uri(authority)).ToArray();
    }

    private bool TryGetAuthorityFromUri(Uri uri, [NotNullWhen(true)] out string? authority)
    {
        try
        {
            if (uri.IsAbsoluteUri)
            {
                authority = uri.GetLeftPart(UriPartial.Authority);
                return true;
            }
        }
        catch (InvalidOperationException)
        {
            // GetLeftPart can throw InvalidOperationException for some URI types
            // Skip malformed URIs
            _logger.LogDebug("Could not extract authority from URI {Uri} as the left part could not be identified, skipping", uri);
        }

        authority = null;
        return false;
    }

    private bool TryGetAuthorityFromUriString(string uriString, [NotNullWhen(true)] out string? authority)
    {
        try
        {
            var existingUri = new Uri(uriString);
            if (TryGetAuthorityFromUri(existingUri, out authority))
            {
                return true;
            }
        }
        catch (UriFormatException)
        {
            // Skip URIs with invalid format
            _logger.LogDebug("Could not extract authority from uriString {String} because of malformed uri format, skipping", uriString);
        }

        authority = null;
        return false;
    }

    internal OpenIddictApplicationDescriptor BackofficeOpenIddictApplicationDescriptor(Uri backOfficeUrl)
        => BackofficeOpenIddictApplicationDescriptor([backOfficeUrl]);

    internal OpenIddictApplicationDescriptor BackofficeOpenIddictApplicationDescriptor(Uri[] backOfficeHosts)
    {
        if (_backOfficeHost is not null)
        {
            backOfficeHosts = [_backOfficeHost];
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            DisplayName = "Umbraco back-office access",
            ClientId = Constants.OAuthClientIds.BackOffice,
            ClientType = OpenIddictConstants.ClientTypes.Public,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.EndSession,
                OpenIddictConstants.Permissions.Endpoints.Revocation,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
            },
        };

        foreach (Uri backOfficeHost in backOfficeHosts)
        {
            descriptor.RedirectUris.Add(CallbackUrlFor(backOfficeHost, _authorizeCallbackPathName));
            descriptor.PostLogoutRedirectUris.Add(CallbackUrlFor(backOfficeHost, _authorizeCallbackPathName));
            descriptor.PostLogoutRedirectUris.Add(CallbackUrlFor(backOfficeHost, _authorizeCallbackLogoutPathName));
        }

        return descriptor;
    }

    internal OpenIddictApplicationDescriptor DeveloperOpenIddictApplicationDescriptor(string name, string clientId, Uri[] redirectUrls)
    {
        var developerClientTimeOutValue = new GlobalSettings().TimeOut.ToString("c", CultureInfo.InvariantCulture);

        var descriptor = new OpenIddictApplicationDescriptor
        {
            DisplayName = name,
            ClientId = clientId,
            ClientType = OpenIddictConstants.ClientTypes.Public,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.ResponseTypes.Code
            },
            Settings =
            {
                // use a fixed access token lifetime for tokens issued to the developer applications.
                [OpenIddictConstants.Settings.TokenLifetimes.AccessToken] = developerClientTimeOutValue
            }
        };

        foreach (Uri redirectUrl in redirectUrls)
        {
            descriptor.RedirectUris.Add(redirectUrl);
        }

        return descriptor;
    }

    private static Uri CallbackUrlFor(Uri url, string relativePath) => new Uri($"{url.GetLeftPart(UriPartial.Authority)}/{relativePath.TrimStart(Constants.CharArrays.ForwardSlash)}");
}
