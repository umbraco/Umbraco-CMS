using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Configuration;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Api.Management.Security;

public class BackOfficeApplicationManager : IBackOfficeApplicationManager
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IRuntimeState _runtimeState;
    private readonly Uri? _backOfficeHost;
    private readonly string? _authorizeCallbackPathName;

    public BackOfficeApplicationManager(
        IOpenIddictApplicationManager applicationManager,
        IWebHostEnvironment webHostEnvironment,
        IOptions<NewBackOfficeSettings> securitySettings,
        IRuntimeState runtimeState)
    {
        _applicationManager = applicationManager;
        _webHostEnvironment = webHostEnvironment;
        _runtimeState = runtimeState;
        _backOfficeHost = securitySettings.Value.BackOfficeHost;
        _authorizeCallbackPathName = securitySettings.Value.AuthorizeCallbackPathName;
    }

    public async Task EnsureBackOfficeApplicationAsync(Uri backOfficeUrl, CancellationToken cancellationToken = default)
    {
        if (_runtimeState.Level < RuntimeLevel.Run)
        {
            return;
        }

        if (backOfficeUrl.IsAbsoluteUri is false)
        {
            throw new ArgumentException($"Expected an absolute URL, got: {backOfficeUrl}", nameof(backOfficeUrl));
        }

        await CreateOrUpdate(
            new OpenIddictApplicationDescriptor
            {
                DisplayName = "Umbraco back-office access",
                ClientId = Constants.OauthClientIds.BackOffice,
                RedirectUris =
                {
                    CallbackUrlFor(_backOfficeHost ?? backOfficeUrl, _authorizeCallbackPathName ?? "/umbraco")
                },
                Type = OpenIddictConstants.ClientTypes.Public,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code
                }
            },
            cancellationToken);

        if (_webHostEnvironment.IsProduction())
        {
            await Delete(Constants.OauthClientIds.Swagger, cancellationToken);
            await Delete(Constants.OauthClientIds.Postman, cancellationToken);
        }
        else
        {
            await CreateOrUpdate(
                new OpenIddictApplicationDescriptor
                {
                    DisplayName = "Umbraco Swagger access",
                    ClientId = Constants.OauthClientIds.Swagger,
                    RedirectUris =
                    {
                        CallbackUrlFor(backOfficeUrl, "/umbraco/swagger/oauth2-redirect.html")
                    },
                    Type = OpenIddictConstants.ClientTypes.Public,
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.ResponseTypes.Code
                    }
                },
                cancellationToken);

            await CreateOrUpdate(
                new OpenIddictApplicationDescriptor
                {
                    DisplayName = "Umbraco Postman access",
                    ClientId = Constants.OauthClientIds.Postman,
                    RedirectUris =
                    {
                        new Uri("https://oauth.pstmn.io/v1/callback"), new Uri("https://oauth.pstmn.io/v1/browser-callback")
                    },
                    Type = OpenIddictConstants.ClientTypes.Public,
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.ResponseTypes.Code
                    }
                },
                cancellationToken);
        }
    }

    private async Task CreateOrUpdate(OpenIddictApplicationDescriptor clientDescriptor, CancellationToken cancellationToken)
    {
        var identifier = clientDescriptor.ClientId ??
                         throw new ApplicationException($"ClientId is missing for application: {clientDescriptor.DisplayName ?? "(no name)"}");
        var client = await _applicationManager.FindByClientIdAsync(identifier, cancellationToken);
        if (client is null)
        {
            await _applicationManager.CreateAsync(clientDescriptor, cancellationToken);
        }
        else
        {
            await _applicationManager.UpdateAsync(client, clientDescriptor, cancellationToken);
        }
    }

    private async Task Delete(string identifier, CancellationToken cancellationToken)
    {
        var client = await _applicationManager.FindByClientIdAsync(identifier, cancellationToken);
        if (client is null)
        {
            return;
        }

        await _applicationManager.DeleteAsync(client, cancellationToken);
    }

    private static Uri CallbackUrlFor(Uri url, string relativePath) => new Uri( $"{url.GetLeftPart(UriPartial.Authority)}/{relativePath.TrimStart(Core.Constants.CharArrays.ForwardSlash)}");
}
