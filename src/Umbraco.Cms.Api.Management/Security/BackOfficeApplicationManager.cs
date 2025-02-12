using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Api.Management.Security;

public class BackOfficeApplicationManager : OpenIdDictApplicationManagerBase, IBackOfficeApplicationManager
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IRuntimeState _runtimeState;
    private readonly Uri? _backOfficeHost;
    private readonly string _authorizeCallbackPathName;
    private readonly string _authorizeCallbackLogoutPathName;

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
    }

    [Obsolete("Please use the overload that allows for multiple back-office hosts. Will be removed in V17.")]
    public async Task EnsureBackOfficeApplicationAsync(Uri backOfficeUrl, CancellationToken cancellationToken = default)
        => await EnsureBackOfficeApplicationAsync([backOfficeUrl], cancellationToken);

    public async Task EnsureBackOfficeApplicationAsync(IEnumerable<Uri> backOfficeHosts, CancellationToken cancellationToken = default)
    {
        // Install is okay without this, because we do not need a token to install,
        // but upgrades do, so we need to execute for everything higher then or equal to upgrade.
        if (_runtimeState.Level < RuntimeLevel.Upgrade)
        {
            return;
        }

        Uri[] backOfficeHostsAsArray = backOfficeHosts as Uri[] ?? backOfficeHosts.ToArray();
        if (backOfficeHostsAsArray.Any(url => url.IsAbsoluteUri) is false)
        {
            throw new ArgumentException($"Expected an absolute URLs, got: {string.Join(", ", backOfficeHostsAsArray.Select(url => url.ToString()))}", nameof(backOfficeHosts));
        }

        await CreateOrUpdate(
            BackofficeOpenIddictApplicationDescriptor(backOfficeHostsAsArray),
            cancellationToken);

        if (_webHostEnvironment.IsProduction())
        {
            await Delete(Constants.OAuthClientIds.Swagger, cancellationToken);
            await Delete(Constants.OAuthClientIds.Postman, cancellationToken);
        }
        else
        {
            await CreateOrUpdate(
                DeveloperOpenIddictApplicationDescriptor(
                    "Umbraco Swagger access",
                    Constants.OAuthClientIds.Swagger,
                    backOfficeHostsAsArray.Select(backOfficeUrl => CallbackUrlFor(backOfficeUrl, "/umbraco/swagger/oauth2-redirect.html")).ToArray()),
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

    [Obsolete("Do not use - for internal usage only. Will be made internal in V17.")]
    public OpenIddictApplicationDescriptor BackofficeOpenIddictApplicationDescriptor(Uri backOfficeUrl)
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
