using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.Common.Security
{
    /// <summary>
    /// The sign in manager for members
    /// </summary>
    public class MemberSignInManager : UmbracoSignInManager<MemberIdentityUser>
    {
        public MemberSignInManager(
            UserManager<MemberIdentityUser> memberManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<MemberIdentityUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<MemberIdentityUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<MemberIdentityUser> confirmation) :
            base(memberManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        { }

        // use default scheme for members
        protected override string AuthenticationType => IdentityConstants.ApplicationScheme;

        // use default scheme for members
        protected override string ExternalAuthenticationType => IdentityConstants.ExternalScheme;

        // use default scheme for members
        protected override string TwoFactorAuthenticationType => IdentityConstants.TwoFactorUserIdScheme;

        // use default scheme for members
        protected override string TwoFactorRememberMeAuthenticationType => IdentityConstants.TwoFactorRememberMeScheme;

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
        public override Task<MemberIdentityUser> GetTwoFactorAuthenticationUserAsync()
            => throw new NotImplementedException("Two factor is not yet implemented for members");

        /// <inheritdoc />
        public override Task<SignInResult> TwoFactorSignInAsync(string provider, string code, bool isPersistent, bool rememberClient)
            => throw new NotImplementedException("Two factor is not yet implemented for members");

        /// <inheritdoc />
        public override Task<bool> IsTwoFactorClientRememberedAsync(MemberIdentityUser user)
            => throw new NotImplementedException("Two factor is not yet implemented for members");

        /// <inheritdoc />
        public override Task RememberTwoFactorClientAsync(MemberIdentityUser user)
            => throw new NotImplementedException("Two factor is not yet implemented for members");

        /// <inheritdoc />
        public override Task ForgetTwoFactorClientAsync()
            => throw new NotImplementedException("Two factor is not yet implemented for members");

        /// <inheritdoc />
        public override Task<SignInResult> TwoFactorRecoveryCodeSignInAsync(string recoveryCode)
            => throw new NotImplementedException("Two factor is not yet implemented for members");

        /// <inheritdoc />
        public override Task<ExternalLoginInfo> GetExternalLoginInfoAsync(string expectedXsrf = null)
            => throw new NotImplementedException("External login is not yet implemented for members");

        /// <inheritdoc />
        public override AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl, string userId = null)
            => throw new NotImplementedException("External login is not yet implemented for members");

        /// <inheritdoc />
        public override Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync()
            => throw new NotImplementedException("External login is not yet implemented for members");

    }
}
