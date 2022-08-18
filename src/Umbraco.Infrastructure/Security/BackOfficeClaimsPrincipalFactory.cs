using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     A <see cref="UserClaimsPrincipalFactory{TUser}"/> for the back office
/// </summary>
public class BackOfficeClaimsPrincipalFactory : UserClaimsPrincipalFactory<BackOfficeIdentityUser>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeClaimsPrincipalFactory" /> class.
    /// </summary>
    /// <param name="userManager">The user manager</param>
    /// <param name="optionsAccessor">The <see cref="BackOfficeIdentityOptions" /></param>
    public BackOfficeClaimsPrincipalFactory(
        UserManager<BackOfficeIdentityUser> userManager,
        IOptions<BackOfficeIdentityOptions> optionsAccessor)
        : base(userManager, optionsAccessor)
    {
    }

    protected virtual string AuthenticationType { get; } = Constants.Security.BackOfficeAuthenticationType;

    /// <inheritdoc />
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BackOfficeIdentityUser user)
    {
        // NOTE: Have a look at the base implementation https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Extensions.Core/src/UserClaimsPrincipalFactory.cs#L79
        // since it's setting an authentication type which is not what we want.
        // so we override this method to change it.

        // get the base
        ClaimsIdentity baseIdentity = await base.GenerateClaimsAsync(user);

        // now create a new one with the correct authentication type
        var id = new ClaimsIdentity(
            AuthenticationType,
            Options.ClaimsIdentity.UserNameClaimType,
            Options.ClaimsIdentity.RoleClaimType);

        // and merge all others from the base implementation
        id.MergeAllClaims(baseIdentity);

        // ensure our required claims are there
        id.AddRequiredClaims(
            user.Id,
            user.UserName,
            user.Name!,
            user.CalculatedContentStartNodeIds,
            user.CalculatedMediaStartNodeIds,
            user.Culture,
            user.SecurityStamp,
            user.AllowedSections,
            user.Roles.Select(x => x.RoleId).ToArray());

        // now we can flow any custom claims that the actual user has currently
        // assigned which could be done in the OnExternalLogin callback
        id.MergeClaimsFromBackOfficeIdentity(user);

        return id;
    }
}
