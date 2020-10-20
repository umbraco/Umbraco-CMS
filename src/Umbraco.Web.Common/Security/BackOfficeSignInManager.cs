using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Extensions;

namespace Umbraco.Web.Common.Security
{
    using Constants = Umbraco.Core.Constants;

    // TODO: There's potential to extract an interface for this for only what we use and put that in Core without aspnetcore refs, but we need to wait till were done with it since there's a bit to implement

    public class BackOfficeSignInManager : SignInManager<BackOfficeIdentityUser>
    {
        private readonly IBackOfficeUserManager _userManager;

        public BackOfficeSignInManager(
            BackOfficeUserManager userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<BackOfficeIdentityUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<BackOfficeIdentityUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<BackOfficeIdentityUser> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            _userManager = userManager;
        }

        // TODO: Need to migrate more from Umbraco.Web.Security.BackOfficeSignInManager
        // Things like dealing with auto-linking, cookie options, and a ton of other stuff. Some might not need to be ported but it
        // will be a case by case basis.
        // Have a look into RefreshSignInAsync since we might be able to use this new functionality for auto-cookie renewal in our middleware, though
        // i suspect it's taken care of already.

        public override async Task<SignInResult> PasswordSignInAsync(BackOfficeIdentityUser user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            // override to handle logging/events
            var result = await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
            return await HandlePasswordSignIn(user, user.UserName, result);
        }

        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            // override to handle logging/events
            var user = await UserManager.FindByNameAsync(userName);
            if (user == null)
                return await HandlePasswordSignIn(null, userName, SignInResult.Failed);
            return await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }

        public override async Task<SignInResult> TwoFactorSignInAsync(string provider, string code, bool isPersistent, bool rememberClient)
        {
            // override to handle logging/events
            var result = await base.TwoFactorSignInAsync(provider, code, isPersistent, rememberClient);
            var user = await GetTwoFactorAuthenticationUserAsync(); // will never be null if the above succeeds
            return await HandlePasswordSignIn(user, user?.UserName, result);
        }

        public override bool IsSignedIn(ClaimsPrincipal principal)
        {
            // override to replace IdentityConstants.ApplicationScheme with Constants.Security.BackOfficeAuthenticationType
            // code taken from aspnetcore: https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs

            if (principal == null) throw new ArgumentNullException(nameof(principal));
            return principal?.Identities != null &&
                principal.Identities.Any(i => i.AuthenticationType == Constants.Security.BackOfficeAuthenticationType);
        }

        public override async Task RefreshSignInAsync(BackOfficeIdentityUser user)
        {
            // override to replace IdentityConstants.ApplicationScheme with Constants.Security.BackOfficeAuthenticationType
            // code taken from aspnetcore: https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs

            var auth = await Context.AuthenticateAsync(Constants.Security.BackOfficeAuthenticationType);
            var claims = new List<Claim>();
            var authenticationMethod = auth?.Principal?.FindFirst(ClaimTypes.AuthenticationMethod);
            if (authenticationMethod != null)
                claims.Add(authenticationMethod);
            var amr = auth?.Principal?.FindFirst("amr");
            if (amr != null)
                claims.Add(amr);
            await SignInWithClaimsAsync(user, auth?.Properties, claims);
        }

        public override async Task SignInWithClaimsAsync(BackOfficeIdentityUser user, AuthenticationProperties authenticationProperties, IEnumerable<Claim> additionalClaims)
        {
            // override to replace IdentityConstants.ApplicationScheme with Constants.Security.BackOfficeAuthenticationType
            // code taken from aspnetcore: https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
            // we also override to set the current HttpContext principal since this isn't done by default

            var userPrincipal = await CreateUserPrincipalAsync(user);
            foreach (var claim in additionalClaims)
                userPrincipal.Identities.First().AddClaim(claim);

            // FYI (just for informational purposes):
            // This calls an ext method will eventually reaches `IAuthenticationService.SignInAsync`
            // which then resolves the `IAuthenticationSignInHandler` for the current scheme
            // by calling `IAuthenticationHandlerProvider.GetHandlerAsync(context, scheme);`
            // which then calls `IAuthenticationSignInHandler.SignInAsync` = CookieAuthenticationHandler.HandleSignInAsync

            // Also note, that when the CookieAuthenticationHandler sign in is successful we handle that event within our
            // own ConfigureUmbracoBackOfficeCookieOptions which assigns the current HttpContext.User to the IPrincipal created

            await Context.SignInAsync(Constants.Security.BackOfficeAuthenticationType,
                userPrincipal,
                authenticationProperties ?? new AuthenticationProperties());
        }

        public override async Task SignOutAsync()
        {
            // override to replace IdentityConstants.ApplicationScheme with Constants.Security.BackOfficeAuthenticationType
            // code taken from aspnetcore: https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs

            await Context.SignOutAsync(Constants.Security.BackOfficeAuthenticationType);
            await Context.SignOutAsync(Constants.Security.BackOfficeExternalAuthenticationType);
            //await Context.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
        }

        private async Task<SignInResult> HandlePasswordSignIn(BackOfficeIdentityUser user, string username, SignInResult result)
        {
            if (username.IsNullOrWhiteSpace())
                username = "UNKNOWN"; // could happen in 2fa or something else weird

            if (result.Succeeded)
            {
                //track the last login date
                user.LastLoginDateUtc = DateTime.UtcNow;
                if (user.AccessFailedCount > 0)
                    //we have successfully logged in, reset the AccessFailedCount
                    user.AccessFailedCount = 0;
                await _userManager.UpdateAsync(user);

                Logger.LogInformation("User: {UserName} logged in from IP address {IpAddress}", username, Context.Connection.RemoteIpAddress);
                if (user != null)
                    _userManager.RaiseLoginSuccessEvent(user, user.Id);
            }
            else if (result.IsLockedOut)
                Logger.LogInformation("Login attempt failed for username {UserName} from IP address {IpAddress}, the user is locked", username, Context.Connection.RemoteIpAddress);
            else if (result.RequiresTwoFactor)
                Logger.LogInformation("Login attempt requires verification for username {UserName} from IP address {IpAddress}", username, Context.Connection.RemoteIpAddress);
            else if (!result.Succeeded || result.IsNotAllowed)
                Logger.LogInformation("Login attempt failed for username {UserName} from IP address {IpAddress}", username, Context.Connection.RemoteIpAddress);
            else
                throw new ArgumentOutOfRangeException();

            return result;
        }
    }
}
