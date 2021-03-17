using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security
{
    //TODO: can any of this be combined/merged with BackOfficeSignInManager using T for the identity user?
    //TODO: Need to implement events on member login/logout etc
    public class MemberSignInManager : SignInManager<MemberIdentityUser>
    {
        private const string ClaimType = "amr";
        private const string PasswordValue = "pwd";
        private readonly IIpResolver _ipResolver;

        public MemberSignInManager(
            UserManager<MemberIdentityUser> memberManager,
            IIpResolver ipResolver,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<MemberIdentityUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<MemberIdentityUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<MemberIdentityUser> confirmation) :
            base(memberManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation) =>
                _ipResolver = ipResolver ?? throw new ArgumentNullException(nameof(ipResolver));

        /// <inheritdoc />
        public override async Task<SignInResult> PasswordSignInAsync(MemberIdentityUser user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            // overridden to handle logging/events
            SignInResult result = await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
            return await HandleSignIn(user, user.UserName, result);
        }

        /// <inheritdoc />
        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            // overridden to handle logging/events
            MemberIdentityUser user = await UserManager.FindByNameAsync(userName);
            if (user == null)
            {
                return await HandleSignIn(null, userName, SignInResult.Failed);
            }

            return await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }

        /// <inheritdoc />
        public override Task<MemberIdentityUser> GetTwoFactorAuthenticationUserAsync() => throw new NotImplementedException("Two factor is not yet implemented for members");

        /// <inheritdoc />
        public override Task<SignInResult> TwoFactorSignInAsync(string provider, string code, bool isPersistent, bool rememberClient) => throw new NotImplementedException("Two factor is not yet implemented for members");

        /// <inheritdoc />
        public override bool IsSignedIn(ClaimsPrincipal principal)
        {
            // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L126
            // replaced in order to use a custom auth type
            // taken from BackOfficeSignInManager

            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            return principal?.Identities != null && principal.Identities.Any(i => i.AuthenticationType == Constants.Security.MemberAuthenticationType);
        }

        /// <inheritdoc />
        public override async Task RefreshSignInAsync(MemberIdentityUser user)
        {
            // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L126
            // replaced in order to use a custom auth type

            AuthenticateResult auth = await Context.AuthenticateAsync(Constants.Security.MemberAuthenticationType);
            IList<Claim> claims = Array.Empty<Claim>();

            Claim authenticationMethod = auth?.Principal?.FindFirst(ClaimTypes.AuthenticationMethod);
            Claim amr = auth?.Principal?.FindFirst(ClaimType);

            if (authenticationMethod != null || amr != null)
            {
                claims = new List<Claim>();
                if (authenticationMethod != null)
                {
                    claims.Add(authenticationMethod);
                }
                if (amr != null)
                {
                    claims.Add(amr);
                }
            }

            await SignInWithClaimsAsync(user, auth?.Properties, claims);
        }

        /// <inheritdoc />
        public override async Task SignInWithClaimsAsync(MemberIdentityUser user, AuthenticationProperties authenticationProperties, IEnumerable<Claim> additionalClaims)
        {
            // TODO: taken from BackOfficeSigninManager and more notes are there
            // override to replace IdentityConstants.ApplicationScheme with Constants.Security.MemberAuthenticationType
            // code taken from aspnetcore: https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
            // we also override to set the current HttpContext principal since this isn't done by default

            ClaimsPrincipal userPrincipal = await CreateUserPrincipalAsync(user);
            foreach (Claim claim in additionalClaims)
            {
                userPrincipal.Identities.First().AddClaim(claim);
            }

            // TODO: For future, this method gets called when performing 2FA logins
            await Context.SignInAsync(Constants.Security.MemberAuthenticationType,
                userPrincipal,
                authenticationProperties ?? new AuthenticationProperties());
        }

        /// <inheritdoc />
        public override async Task SignOutAsync() =>
            //TODO: does members need this custom signout type as per BackOfficeSignInManager?
            // override to replace IdentityConstants.ApplicationScheme with Constants.Security.MemberAuthenticationType
            // code taken from aspnetcore: https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
            await Context.SignOutAsync(Constants.Security.MemberAuthenticationType);

        /// <inheritdoc />
        public override Task<bool> IsTwoFactorClientRememberedAsync(MemberIdentityUser user) => throw new NotImplementedException("Two factor is not yet implemented for members");

        /// <inheritdoc />
        public override Task RememberTwoFactorClientAsync(MemberIdentityUser user) => throw new NotImplementedException("Two factor is not yet implemented for members");

        /// <inheritdoc />
        public override Task ForgetTwoFactorClientAsync() => throw new NotImplementedException("Two factor is not yet implemented for members");

        /// <inheritdoc />
        public override Task<SignInResult> TwoFactorRecoveryCodeSignInAsync(string recoveryCode) => throw new NotImplementedException("Two factor is not yet implemented for members");

        /// <inheritdoc />
        public override Task<ExternalLoginInfo> GetExternalLoginInfoAsync(string expectedXsrf = null) => throw new NotImplementedException("External login is not yet implemented for members");

        /// <inheritdoc />
        public override AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl, string userId = null) => throw new NotImplementedException("External login is not yet implemented for members");

        /// <inheritdoc />
        public override Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync() => throw new NotImplementedException("External login is not yet implemented for members");

        /// <inheritdoc />
        /// TODO: Two factor is not yet implemented for members
        protected override async Task<SignInResult> SignInOrTwoFactorAsync(MemberIdentityUser user, bool isPersistent, string loginProvider = null, bool bypassTwoFactor = false)
        {
            // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
            // to replace custom auth types

            //TODO: There is currently no two factor so this needs changing once implemented
            if (loginProvider != null)
            {
                await SignInAsync(user, isPersistent, loginProvider);
            }
            else
            {
                await SignInWithClaimsAsync(user, isPersistent, new Claim[]
                {
                    new Claim(ClaimType, PasswordValue)
                });
            }
            return SignInResult.Success;
        }

        /// <summary>
        /// Called on any login attempt to update the AccessFailedCount and to raise events
        /// </summary>
        /// <param name="user"></param>
        /// <param name="username"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task<SignInResult> HandleSignIn(MemberIdentityUser user, string username, SignInResult result)
        {
            // TODO: More TODO notes in BackOfficeSignInManager
            if (username.IsNullOrWhiteSpace())
            {
                //TODO: this might have unwanted effects if the member is called this
                username = "UNKNOWN";
            }

            if (result.Succeeded)
            {
                //track the last login date
                user.LastLoginDateUtc = DateTime.UtcNow;
                if (user.AccessFailedCount > 0)
                {
                    //we have successfully logged in, reset the AccessFailedCount
                    user.AccessFailedCount = 0;
                }
                await UserManager.UpdateAsync(user);

                Logger.LogInformation("User: {UserName} logged in from IP address {IpAddress}", username, _ipResolver.GetCurrentRequestIpAddress());
                if (user != null)
                {
                    //TODO: what events do we want for members?
                    //_memberManager.RaiseLoginSuccessEvent(Context.User, user.Id);
                }
            }
            else if (result.IsLockedOut)
            {
                //TODO: what events do we want for members?
                //_memberManager.RaiseAccountLockedEvent(Context.User, user.Id);
                Logger.LogInformation("Login attempt failed for username {UserName} from IP address {IpAddress}, the user is locked", username, _ipResolver.GetCurrentRequestIpAddress());
            }
            else if (result.RequiresTwoFactor)
            {
                //TODO: what events do we want for members?
                //_memberManager.RaiseLoginRequiresVerificationEvent(Context.User, user.Id);
                Logger.LogInformation("Login attempt requires verification for username {UserName} from IP address {IpAddress}", username, _ipResolver.GetCurrentRequestIpAddress());
            }
            else if (!result.Succeeded || result.IsNotAllowed)
            {
                Logger.LogInformation("Login attempt failed for username {UserName} from IP address {IpAddress}", username, _ipResolver.GetCurrentRequestIpAddress());
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            return result;
        }
    }
}
