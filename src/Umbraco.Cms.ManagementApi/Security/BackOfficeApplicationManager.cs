using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Umbraco.New.Cms.Core;
using Umbraco.New.Cms.Core.Models.Configuration;
using Umbraco.New.Cms.Infrastructure.Security;

namespace Umbraco.Cms.ManagementApi.Security;

public class BackOfficeApplicationManager : IBackOfficeApplicationManager
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IClientSecretManager _clientSecretManager;
    private readonly Uri? _backOfficeHost;

    public BackOfficeApplicationManager(
        IOpenIddictApplicationManager applicationManager,
        IWebHostEnvironment webHostEnvironment,
        IClientSecretManager clientSecretManager,
        IOptionsMonitor<NewBackOfficeSettings> securitySettingsMonitor)
    {
        _applicationManager = applicationManager;
        _webHostEnvironment = webHostEnvironment;
        _clientSecretManager = clientSecretManager;
        _backOfficeHost = securitySettingsMonitor.CurrentValue.BackOfficeHost;
    }

    public async Task EnsureBackOfficeApplicationAsync(Uri backOfficeUrl, CancellationToken cancellationToken = default)
    {
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
                    CallbackUrlFor(_backOfficeHost ?? backOfficeUrl, "/umbraco/login/callback/")
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
                    // TODO: investigate the necessity of client secrets for Swagger
                    // this is necessary with NSwag - or maybe it's a SwaggerUI3 requirement? investigate if client
                    // secrets are even necessary if we switch to Swashbuckle
                    ClientSecret = _clientSecretManager.Get(Constants.OauthClientIds.Swagger),
                    RedirectUris =
                    {
                        CallbackUrlFor(backOfficeUrl, "/umbraco/swagger/oauth2-redirect.html")
                    },
                    Type = OpenIddictConstants.ClientTypes.Confidential,
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
                        new Uri("https://oauth.pstmn.io/v1/callback")
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
