using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security;

/// <summary>
///     A <see cref="UserClaimsPrincipalFactory{TUser}"/> for members
/// </summary>
public class MemberClaimsPrincipalFactory : UserClaimsPrincipalFactory<MemberIdentityUser>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeClaimsPrincipalFactory" /> class.
    /// </summary>
    /// <param name="userManager">The user manager</param>
    /// <param name="optionsAccessor">The <see cref="BackOfficeIdentityOptions" /></param>
    public MemberClaimsPrincipalFactory(
        UserManager<MemberIdentityUser> userManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, optionsAccessor)
    {
    }

    protected virtual string AuthenticationType => IdentityConstants.ApplicationScheme;

    /// <inheritdoc />
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(MemberIdentityUser member)
    {
        // Get the base
        ClaimsIdentity baseIdentity = await base.GenerateClaimsAsync(member);

        // now create a new one with the correct authentication type
        var memberIdentity = new ClaimsIdentity(
            AuthenticationType,
            Options.ClaimsIdentity.UserNameClaimType,
            Options.ClaimsIdentity.RoleClaimType);

        // and merge all others from the base implementation
        memberIdentity.MergeAllClaims(baseIdentity);

        // And merge claims added to the user, for instance in OnExternalLogin, we need to do this explicitly, since the claims are IdentityClaims, so it's not handled by memberIdentity.
        foreach (Claim claim in member.Claims
                     .Where(claim => memberIdentity.HasClaim(claim.ClaimType, claim.ClaimValue) is false)
                     .Select(x => new Claim(x.ClaimType, x.ClaimValue)))
        {
            memberIdentity.AddClaim(claim);
        }

        if(string.IsNullOrEmpty(member.Name) == false)
        {
            // Updates/overwrites the name claim that is storing the username/email address
            // to actually be the member's name
            memberIdentity.AddOrUpdateClaim(new Claim(ClaimTypes.Name, member.Name));

            // Add additional claim if people wish to be more explict 
            // and call the extension method GetRealname()
            memberIdentity.AddOrUpdateClaim(new Claim(ClaimTypes.GivenName, member.Name));
        }

        return memberIdentity;
    }
}
