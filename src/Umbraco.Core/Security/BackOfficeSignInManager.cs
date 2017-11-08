using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
{
    //TODO: In v8 we need to change this to use an int? nullable TKey instead, see notes against overridden TwoFactorSignInAsync
    public class BackOfficeSignInManager : SignInManager<BackOfficeIdentityUser, int>
    {
        private readonly ILogger _logger;
        private readonly IOwinRequest _request;

        public BackOfficeSignInManager(UserManager<BackOfficeIdentityUser, int> userManager, IAuthenticationManager authenticationManager, ILogger logger, IOwinRequest request)
            : base(userManager, authenticationManager)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (request == null) throw new ArgumentNullException("request");
            _logger = logger;
            _request = request;
            AuthenticationType = Constants.Security.BackOfficeAuthenticationType;
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(BackOfficeIdentityUser user)
        {
            return user.GenerateUserIdentityAsync((BackOfficeUserManager<BackOfficeIdentityUser>)UserManager);
        }

        public static BackOfficeSignInManager Create(IdentityFactoryOptions<BackOfficeSignInManager> options, IOwinContext context, ILogger logger)
        {
            return new BackOfficeSignInManager(
                context.GetBackOfficeUserManager(),
                context.Authentication,
                logger,
                context.Request);
        }

        /// <summary>
        /// Sign in the user in using the user name and password
        /// </summary>
        /// <param name="userName"/><param name="password"/><param name="isPersistent"/><param name="shouldLockout"/>
        /// <returns/>
        public override async Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            var result = await PasswordSignInAsyncImpl(userName, password, isPersistent, shouldLockout);

            switch (result)
            {
                case SignInStatus.Success:
                    _logger.WriteCore(TraceEventType.Information, 0,
                        string.Format(
                            "User: {0} logged in from IP address {1}",
                            userName,
                            _request.RemoteIpAddress), null, null);
                    break;
                case SignInStatus.LockedOut:
                    _logger.WriteCore(TraceEventType.Information, 0,
                        string.Format(
                            "Login attempt failed for username {0} from IP address {1}, the user is locked",
                            userName,
                            _request.RemoteIpAddress), null, null);
                    break;
                case SignInStatus.RequiresVerification:
                    _logger.WriteCore(TraceEventType.Information, 0,
                        string.Format(
                            "Login attempt requires verification for username {0} from IP address {1}",
                            userName,
                            _request.RemoteIpAddress), null, null);
                    break;
                case SignInStatus.Failure:
                    _logger.WriteCore(TraceEventType.Information, 0,
                        string.Format(
                            "Login attempt failed for username {0} from IP address {1}",
                            userName,
                            _request.RemoteIpAddress), null, null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        /// <summary>
        /// Borrowed from Micorosoft's underlying sign in manager which is not flexible enough to tell it to use a different cookie type
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="isPersistent"></param>
        /// <param name="shouldLockout"></param>
        /// <returns></returns>
        private async Task<SignInStatus> PasswordSignInAsyncImpl(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            if (UserManager == null)
            {
                return SignInStatus.Failure;
            }

            var user = await UserManager.FindByNameAsync(userName);
            
            //if the user is null, create an empty one which can be used for auto-linking
            if (user == null)            
                user = BackOfficeIdentityUser.CreateNew(userName, null, GlobalSettings.DefaultUILanguage);            
            
            //check the password for the user, this will allow a developer to auto-link 
            //an account if they have specified an IBackOfficeUserPasswordChecker
            if (await UserManager.CheckPasswordAsync(user, password))
            {
                //the underlying call to this will query the user by Id which IS cached!
                if (await UserManager.IsLockedOutAsync(user.Id))
                {
                    return SignInStatus.LockedOut;
                }

                await UserManager.ResetAccessFailedCountAsync(user.Id);
                return await SignInOrTwoFactor(user, isPersistent);
            }

            var requestContext = _request.Context;

            if (user.HasIdentity && shouldLockout)
            {
                // If lockout is requested, increment access failed count which might lock out the user
                await UserManager.AccessFailedAsync(user.Id);
                if (await UserManager.IsLockedOutAsync(user.Id))
                {
                    //at this point we've just locked the user out after too many failed login attempts
                    
                    if (requestContext != null)
                    {
                        var backofficeUserManager = requestContext.GetBackOfficeUserManager();
                        if (backofficeUserManager != null)
                            backofficeUserManager.RaiseAccountLockedEvent(user.Id);
                    }

                    return SignInStatus.LockedOut;
                }
            }
            
            if (requestContext != null)
            {
                var backofficeUserManager = requestContext.GetBackOfficeUserManager();
                if (backofficeUserManager != null)
                    backofficeUserManager.RaiseInvalidLoginAttemptEvent(userName);
            }

            return SignInStatus.Failure;
        }        

        /// <summary>
        /// Borrowed from Micorosoft's underlying sign in manager which is not flexible enough to tell it to use a different cookie type
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isPersistent"></param>
        /// <returns></returns>
        private async Task<SignInStatus> SignInOrTwoFactor(BackOfficeIdentityUser user, bool isPersistent)
        {
            var id = Convert.ToString(user.Id);
            if (await UserManager.GetTwoFactorEnabledAsync(user.Id)
                && (await UserManager.GetValidTwoFactorProvidersAsync(user.Id)).Count > 0)
            {
                var identity = new ClaimsIdentity(Constants.Security.BackOfficeTwoFactorAuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id));
                identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName));
                AuthenticationManager.SignIn(identity);
                return SignInStatus.RequiresVerification;
            }
            await SignInAsync(user, isPersistent, false);
            return SignInStatus.Success;
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

            //track the last login date
            user.LastLoginDateUtc = DateTime.UtcNow;
            if (user.AccessFailedCount > 0)
                //we have successfully logged in, reset the AccessFailedCount
                user.AccessFailedCount = 0;
            await UserManager.UpdateAsync(user);

            _logger.WriteCore(TraceEventType.Information, 0,
                string.Format(
                    "Login attempt succeeded for username {0} from IP address {1}",
                    user.UserName,
                    _request.RemoteIpAddress), null, null);
        }

        /// <summary>
        /// Get the user id that has been verified already or -1.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Replaces the underlying call which is not flexible and doesn't support a custom cookie
        /// </remarks>
        public new async Task<int> GetVerifiedUserIdAsync()
        {
            var result = await AuthenticationManager.AuthenticateAsync(Constants.Security.BackOfficeTwoFactorAuthenticationType);
            if (result != null && result.Identity != null && string.IsNullOrEmpty(result.Identity.GetUserId()) == false)
            {
                return ConvertIdFromString(result.Identity.GetUserId());
            }
            return -1;
        }

        /// <summary>
        /// Get the username that has been verified already or null.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetVerifiedUserNameAsync()
        {
            var result = await AuthenticationManager.AuthenticateAsync(Constants.Security.BackOfficeTwoFactorAuthenticationType);
            if (result != null && result.Identity != null && string.IsNullOrEmpty(result.Identity.GetUserName()) == false)
            {
                return result.Identity.GetUserName();
            }
            return null;
        }

        /// <summary>
        /// Two factor verification step
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="code"></param>
        /// <param name="isPersistent"></param>
        /// <param name="rememberBrowser"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is implemented because we cannot override GetVerifiedUserIdAsync and instead we have to shadow it
        /// so due to this and because we are using an INT as the TKey and not an object, it can never be null. Adding to that
        /// the default(int) value returned by the base class is always a valid user (i.e. the admin) so we just have to duplicate
        /// all of this code to check for -1 instead.
        /// </remarks>
        public override async Task<SignInStatus> TwoFactorSignInAsync(string provider, string code, bool isPersistent, bool rememberBrowser)
        {
            var userId = await GetVerifiedUserIdAsync();
            if (userId == -1)
            {
                return SignInStatus.Failure;
            }
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return SignInStatus.Failure;
            }
            if (await UserManager.IsLockedOutAsync(user.Id))
            {
                return SignInStatus.LockedOut;
            }
            if (await UserManager.VerifyTwoFactorTokenAsync(user.Id, provider, code))
            {
                // When token is verified correctly, clear the access failed count used for lockout
                await UserManager.ResetAccessFailedCountAsync(user.Id);
                await SignInAsync(user, isPersistent, rememberBrowser);
                return SignInStatus.Success;
            }
            // If the token is incorrect, record the failure which also may cause the user to be locked out
            await UserManager.AccessFailedAsync(user.Id);
            return SignInStatus.Failure;
        }

        /// <summary>Send a two factor code to a user</summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is implemented because we cannot override GetVerifiedUserIdAsync and instead we have to shadow it
        /// so due to this and because we are using an INT as the TKey and not an object, it can never be null. Adding to that
        /// the default(int) value returned by the base class is always a valid user (i.e. the admin) so we just have to duplicate
        /// all of this code to check for -1 instead.
        /// </remarks>
        public override async Task<bool> SendTwoFactorCodeAsync(string provider)
        {
            var userId = await GetVerifiedUserIdAsync();
            if (userId == -1)
                return false;

            var token = await UserManager.GenerateTwoFactorTokenAsync(userId, provider);
            var identityResult = await UserManager.NotifyTwoFactorTokenAsync(userId, provider, token);
            return identityResult.Succeeded;
        }
    }
}
