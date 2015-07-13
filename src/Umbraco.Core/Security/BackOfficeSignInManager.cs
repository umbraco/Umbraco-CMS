using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
{
    public class BackOfficeSignInManager : SignInManager<BackOfficeIdentityUser, int>
    {
        public BackOfficeSignInManager(BackOfficeUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
            AuthenticationType = Constants.Security.BackOfficeAuthenticationType;
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(BackOfficeIdentityUser user)
        {
            return user.GenerateUserIdentityAsync((BackOfficeUserManager)UserManager);
        }

        public static BackOfficeSignInManager Create(IdentityFactoryOptions<BackOfficeSignInManager> options, IOwinContext context)
        {
            return new BackOfficeSignInManager(context.GetUserManager<BackOfficeUserManager>(), context.Authentication);
        }

        /// <summary>
        /// Creates a user identity and then signs the identity using the AuthenticationManager
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isPersistent"></param>
        /// <param name="rememberBrowser"></param>
        /// <returns></returns>
        public override async Task SignInAsync(BackOfficeIdentityUser user, bool isPersistent, bool rememberBrowser)
        {
            var userIdentity = await CreateUserIdentityAsync(user);

            // Clear any partial cookies from external or two factor partial sign ins
            AuthenticationManager.SignOut(
                Constants.Security.BackOfficeExternalAuthenticationType, 
                Constants.Security.BackOfficeTwoFactorAuthenticationType);

            var nowUtc = DateTime.Now.ToUniversalTime();
            
            if (rememberBrowser)
            {
                var rememberBrowserIdentity = AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(ConvertIdToString(user.Id));
                AuthenticationManager.SignIn(new AuthenticationProperties()
                {
                    IsPersistent = isPersistent,
                    AllowRefresh = true,
                    IssuedUtc = nowUtc,
                    ExpiresUtc = nowUtc.AddMinutes(GlobalSettings.TimeOutInMinutes)
                }, userIdentity, rememberBrowserIdentity);
            }
            else
            {
                AuthenticationManager.SignIn(new AuthenticationProperties()
                {
                    IsPersistent = isPersistent,
                    AllowRefresh = true,
                    IssuedUtc = nowUtc,
                    ExpiresUtc = nowUtc.AddMinutes(GlobalSettings.TimeOutInMinutes)
                }, userIdentity);
            }
        }
    }
}