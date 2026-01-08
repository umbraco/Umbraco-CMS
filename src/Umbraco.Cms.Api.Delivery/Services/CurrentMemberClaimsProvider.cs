using OpenIddict.Abstractions;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.Delivery.Services;

// NOTE: this is public and unsealed to allow overriding the default claims with minimal effort.
public class CurrentMemberClaimsProvider : ICurrentMemberClaimsProvider
{
    private readonly IMemberManager _memberManager;

    public CurrentMemberClaimsProvider(IMemberManager memberManager)
        => _memberManager = memberManager;

    public virtual async Task<Dictionary<string, object>> GetClaimsAsync()
    {
        MemberIdentityUser? memberIdentityUser = await _memberManager.GetCurrentMemberAsync();
        return memberIdentityUser is not null
            ? await GetClaimsForMemberIdentityAsync(memberIdentityUser)
            : throw new InvalidOperationException("Could not retrieve the current member. This method should only ever be invoked when a member has been authorized.");
    }

    protected virtual async Task<Dictionary<string, object>> GetClaimsForMemberIdentityAsync(MemberIdentityUser memberIdentityUser)
    {
        var claims = new Dictionary<string, object>
        {
            [OpenIddictConstants.Claims.Subject] = memberIdentityUser.Key
        };

        if (memberIdentityUser.Name is not null)
        {
            claims[OpenIddictConstants.Claims.Name] = memberIdentityUser.Name;
        }

        if (memberIdentityUser.Email is not null)
        {
            claims[OpenIddictConstants.Claims.Email] = memberIdentityUser.Email;
        }

        claims[OpenIddictConstants.Claims.Role] = await _memberManager.GetRolesAsync(memberIdentityUser);

        return claims;
    }
}
