using OpenIddict.Abstractions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Api.Delivery.Security;

public class MemberApplicationManager : OpenIdDictApplicationManagerBase, IMemberApplicationManager
{
    private readonly IRuntimeState _runtimeState;

    public MemberApplicationManager(IOpenIddictApplicationManager applicationManager, IRuntimeState runtimeState)
        : base(applicationManager)
        => _runtimeState = runtimeState;

    public async Task EnsureMemberApplicationAsync(IEnumerable<Uri> loginRedirectUrls, IEnumerable<Uri> logoutRedirectUrls, CancellationToken cancellationToken = default)
    {
        if (_runtimeState.Level < RuntimeLevel.Run)
        {
            return;
        }

        Uri[] loginRedirectUrlsArray = loginRedirectUrls as Uri[] ?? loginRedirectUrls.ToArray();
        if (loginRedirectUrlsArray.All(r => r.IsAbsoluteUri) is false)
        {
            throw new ArgumentException("Expected absolute login redirect URLs for Delivery API member authentication", nameof(loginRedirectUrls));
        }

        Uri[] logoutRedirectUrlsArray = logoutRedirectUrls as Uri[] ?? logoutRedirectUrls.ToArray();
        if (logoutRedirectUrlsArray.All(r => r.IsAbsoluteUri) is false)
        {
            throw new ArgumentException("Expected absolute logout redirect URLs for Delivery API member authentication", nameof(logoutRedirectUrlsArray));
        }

        var applicationDescriptor = new OpenIddictApplicationDescriptor
        {
            DisplayName = "Umbraco member access",
            ClientId = Constants.OAuthClientIds.Member,
            Type = OpenIddictConstants.ClientTypes.Public,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.Logout,
                OpenIddictConstants.Permissions.Endpoints.Revocation,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.ResponseTypes.Code
            }
        };

        foreach (Uri redirectUrl in loginRedirectUrlsArray)
        {
            applicationDescriptor.RedirectUris.Add(redirectUrl);
        }

        foreach (Uri redirectUrl in logoutRedirectUrlsArray)
        {
            applicationDescriptor.PostLogoutRedirectUris.Add(redirectUrl);
        }

        await CreateOrUpdate(applicationDescriptor, cancellationToken);
    }

    public async Task DeleteMemberApplicationAsync(CancellationToken cancellationToken = default)
        => await Delete(Constants.OAuthClientIds.Member, cancellationToken);
}
