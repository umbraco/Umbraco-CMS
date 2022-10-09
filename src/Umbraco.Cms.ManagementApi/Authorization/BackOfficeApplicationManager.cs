using OpenIddict.Abstractions;

namespace Umbraco.Cms.ManagementApi.Authorization;

public class BackOfficeApplicationManager : IBackOfficeApplicationManager
{
    private const string BackOfficeClientId = "umbraco-back-office";

    private readonly IOpenIddictApplicationManager _applicationManager;

    public BackOfficeApplicationManager(IOpenIddictApplicationManager applicationManager)
        => _applicationManager = applicationManager;

    public async Task EnsureBackOfficeApplicationAsync(Uri url, CancellationToken cancellationToken = default)
    {
        if (url.IsAbsoluteUri == false)
        {
            throw new ArgumentException($"Expected an absolute URL, got: {url}", nameof(url));
        }

        var clientDescriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = BackOfficeClientId,
            RedirectUris = { CallbackUrlFor(url) },
            Type = OpenIddictConstants.ClientTypes.Public,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.ResponseTypes.Code
            }
        };

        var client = await _applicationManager.FindByClientIdAsync(BackOfficeClientId, cancellationToken);
        if (client == null)
        {
            await _applicationManager.CreateAsync(clientDescriptor, cancellationToken);
        }
        else
        {
            await _applicationManager.UpdateAsync(client, clientDescriptor, cancellationToken);
        }
    }

    private static Uri CallbackUrlFor(Uri url) => new Uri( $"{url.GetLeftPart(UriPartial.Authority)}/umbraco/login/callback/");
}
