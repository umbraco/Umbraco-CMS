using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Owin.Security;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Core.Services;
using Umbraco.Web.Features;
using Umbraco.Web.Security;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Hosting;
using BackOfficeIdentityUser = Umbraco.Core.BackOffice.BackOfficeIdentityUser;

namespace Umbraco.Web.Editors
{

    /// <summary>
    /// Represents a controller user to render out the default back office view and JS results.
    /// </summary>
    [UmbracoRequireHttps]
  //  [DisableBrowserCache]
    public class BackOfficeController : Controller
    {
        private BackOfficeOwinUserManager _userManager;
        private BackOfficeSignInManager _signInManager;
        private readonly IOptions<GlobalSettings> _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IUserService _userService;
        private readonly ILogger<BackOfficeController> _logger;

        public BackOfficeController(
            IOptions<GlobalSettings> globalSettings,
            ILoggerFactory loggerFactory,
            IHostingEnvironment hostingEnvironment,
            IUmbracoContextAccessor umbracoContextAccessor,
            IUserService userService
           )
        {
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
            _umbracoContextAccessor = umbracoContextAccessor;
            _userService = userService;
            _logger = loggerFactory.CreateLogger<BackOfficeController>();
        }

        protected BackOfficeSignInManager SignInManager => null;

        protected BackOfficeOwinUserManager UserManager => null;

        protected IAuthenticationManager AuthenticationManager => null;


        // TODO: for converting to netcore, some examples:
        // * https://github.com/dotnet/aspnetcore/blob/master/src/Identity/samples/IdentitySample.Mvc/Controllers/AccountController.cs
        // * https://github.com/dotnet/aspnetcore/blob/master/src/MusicStore/samples/MusicStore/Controllers/AccountController.cs
        [HttpPost]
        public ActionResult ExternalLogin(string provider, string redirectUrl = null)
        {
            if (redirectUrl == null)
            {
                redirectUrl = Url.Action("Default", "BackOffice");
            }

            // Request a redirect to the external login provider
            return new ChallengeResult(provider, redirectUrl);
        }

        // TODO: for converting to netcore, some examples:
        // * https://github.com/dotnet/aspnetcore/blob/master/src/Identity/samples/IdentitySample.Mvc/Controllers/AccountController.cs
        // * https://github.com/dotnet/aspnetcore/blob/master/src/MusicStore/samples/MusicStore/Controllers/AccountController.cs
        [UmbracoAuthorize]
        [HttpPost]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider,
                Url.Action("ExternalLinkLoginCallback", "BackOffice"),
                User.Identity.GetUserId());
        }

        // TODO: for converting to netcore, some examples:
        // * https://github.com/dotnet/aspnetcore/blob/master/src/Identity/samples/IdentitySample.Mvc/Controllers/AccountController.cs
        // * https://github.com/dotnet/aspnetcore/blob/master/src/MusicStore/samples/MusicStore/Controllers/AccountController.cs
        [HttpGet]
        public async Task<ActionResult> ExternalLinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(
                Constants.Security.BackOfficeExternalAuthenticationType,
                XsrfKey, User.Identity.GetUserId());

            if (loginInfo == null)
            {
                //Add error and redirect for it to be displayed
                TempData[ViewDataExtensions.TokenExternalSignInError] = new[] { "An error occurred, could not get external login info" };
                return RedirectToLocal(Url.Action("Default", "BackOffice"));
            }

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null) throw new InvalidOperationException("Could not find user");

            var result = await UserManager.AddLoginAsync(user,
                new UserLoginInfo(loginInfo.Login.LoginProvider, loginInfo.Login.ProviderKey, loginInfo.Login.LoginProvider));
            if (result.Succeeded)
            {
                return RedirectToLocal(Url.Action("Default", "BackOffice"));
            }

            //Add errors and redirect for it to be displayed
            TempData[ViewDataExtensions.TokenExternalSignInError] = result.Errors;
            return RedirectToLocal(Url.Action("Default", "BackOffice"));
        }

        /// <summary>
        /// Used by Default and AuthorizeUpgrade to render as per normal if there's no external login info,
        /// otherwise process the external login info.
        /// </summary>
        /// <returns></returns>
        private async Task<ActionResult> RenderDefaultOrProcessExternalLoginAsync(
            Func<ActionResult> defaultResponse,
            Func<ActionResult> externalSignInResponse)
        {
            if (defaultResponse == null) throw new ArgumentNullException("defaultResponse");
            if (externalSignInResponse == null) throw new ArgumentNullException("externalSignInResponse");

            ViewData.SetUmbracoPath(_globalSettings.Value.GetUmbracoMvcArea(_hostingEnvironment));

            //check if there is the TempData with the any token name specified, if so, assign to view bag and render the view
            if (ViewData.FromTempData(TempData, ViewDataExtensions.TokenExternalSignInError) ||
                ViewData.FromTempData(TempData, ViewDataExtensions.TokenPasswordResetCode))
                return defaultResponse();

            //First check if there's external login info, if there's not proceed as normal
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(
                Constants.Security.BackOfficeExternalAuthenticationType);

            if (loginInfo == null || loginInfo.ExternalIdentity.IsAuthenticated == false)
            {
                return defaultResponse();
            }

            //we're just logging in with an external source, not linking accounts
            return await ExternalSignInAsync(loginInfo, externalSignInResponse);
        }

        // TODO: for converting to netcore, some examples:
        // * https://github.com/dotnet/aspnetcore/blob/master/src/Identity/samples/IdentitySample.Mvc/Controllers/AccountController.cs
        // * https://github.com/dotnet/aspnetcore/blob/master/src/MusicStore/samples/MusicStore/Controllers/AccountController.cs
        private async Task<ActionResult> ExternalSignInAsync(ExternalLoginInfo loginInfo, Func<ActionResult> response)
        {
            if (loginInfo == null) throw new ArgumentNullException("loginInfo");
            if (response == null) throw new ArgumentNullException("response");
            ExternalSignInAutoLinkOptions autoLinkOptions = null;

            //Here we can check if the provider associated with the request has been configured to allow
            // new users (auto-linked external accounts). This would never be used with public providers such as
            // Google, unless you for some reason wanted anybody to be able to access the backend if they have a Google account
            // .... not likely!
            var authType = AuthenticationManager.GetExternalAuthenticationTypes().FirstOrDefault(x => x.AuthenticationType == loginInfo.Login.LoginProvider);
            if (authType == null)
            {
                _logger.LogWarning("Could not find external authentication provider registered: {LoginProvider}", loginInfo.Login.LoginProvider);
            }
            else
            {
                autoLinkOptions = authType.GetExternalAuthenticationOptions();
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindByLoginAsync(loginInfo.Login.LoginProvider, loginInfo.Login.ProviderKey);
            if (user != null)
            {
                // TODO: It might be worth keeping some of the claims associated with the ExternalLoginInfo, in which case we
                // wouldn't necessarily sign the user in here with the standard login, instead we'd update the
                // UseUmbracoBackOfficeExternalCookieAuthentication extension method to have the correct provider and claims factory,
                // ticket format, etc.. to create our back office user including the claims assigned and in this method we'd just ensure
                // that the ticket is created and stored and that the user is logged in.

                var shouldSignIn = true;
                if (autoLinkOptions != null && autoLinkOptions.OnExternalLogin != null)
                {
                    shouldSignIn = autoLinkOptions.OnExternalLogin(user, loginInfo);
                    if (shouldSignIn == false)
                    {
                        _logger.LogWarning("The AutoLinkOptions of the external authentication provider '{LoginProvider}' have refused the login based on the OnExternalLogin method. Affected user id: '{UserId}'", loginInfo.Login.LoginProvider, user.Id);
                    }
                }

                if (shouldSignIn)
                {
                    //sign in
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
            }
            else
            {
                if (await AutoLinkAndSignInExternalAccount(loginInfo, autoLinkOptions) == false)
                {
                    ViewData.SetExternalSignInError(new[] { "The requested provider (" + loginInfo.Login.LoginProvider + ") has not been linked to an account" });
                }

                //Remove the cookie otherwise this message will keep appearing
                if (Response.Cookies[Constants.Security.BackOfficeExternalCookieName] != null)
                {
                    Response.Cookies[Constants.Security.BackOfficeExternalCookieName].Expires = DateTime.MinValue;
                }
            }

            return response();
        }

        private async Task<bool> AutoLinkAndSignInExternalAccount(ExternalLoginInfo loginInfo, ExternalSignInAutoLinkOptions autoLinkOptions)
        {
            if (autoLinkOptions == null)
                return false;

            if (autoLinkOptions.ShouldAutoLinkExternalAccount(_umbracoContextAccessor.UmbracoContext, loginInfo) == false)
                return true;

            //we are allowing auto-linking/creating of local accounts
            if (loginInfo.Email.IsNullOrWhiteSpace())
            {
                ViewData.SetExternalSignInError(new[] { "The requested provider (" + loginInfo.Login.LoginProvider + ") has not provided an email address, the account cannot be linked." });
            }
            else
            {
                //Now we need to perform the auto-link, so first we need to lookup/create a user with the email address
                var foundByEmail = _userService.GetByEmail(loginInfo.Email);
                if (foundByEmail != null)
                {
                    ViewData.SetExternalSignInError(new[] { "A user with this email address already exists locally. You will need to login locally to Umbraco and link this external provider: " + loginInfo.Login.LoginProvider });
                }
                else
                {
                    if (loginInfo.Email.IsNullOrWhiteSpace()) throw new InvalidOperationException("The Email value cannot be null");
                    if (loginInfo.ExternalIdentity.Name.IsNullOrWhiteSpace()) throw new InvalidOperationException("The Name value cannot be null");

                    var groups = _userService.GetUserGroupsByAlias(autoLinkOptions.GetDefaultUserGroups(_umbracoContextAccessor.UmbracoContext, loginInfo));

                    var autoLinkUser = BackOfficeIdentityUser.CreateNew(_globalSettings.Value,
                        loginInfo.Email,
                        loginInfo.Email,
                        autoLinkOptions.GetDefaultCulture(_umbracoContextAccessor.UmbracoContext, loginInfo));
                    autoLinkUser.Name = loginInfo.ExternalIdentity.Name;
                    foreach (var userGroup in groups)
                    {
                        autoLinkUser.AddRole(userGroup.Alias);
                    }

                    //call the callback if one is assigned
                    if (autoLinkOptions.OnAutoLinking != null)
                    {
                        autoLinkOptions.OnAutoLinking(autoLinkUser, loginInfo);
                    }

                    var userCreationResult = await UserManager.CreateAsync(autoLinkUser);

                    if (userCreationResult.Succeeded == false)
                    {
                        ViewData.SetExternalSignInError(userCreationResult.Errors.Select(x => x.Description).ToList());
                    }
                    else
                    {
                        var linkResult = await UserManager.AddLoginAsync(autoLinkUser,
                            new UserLoginInfo(loginInfo.Login.LoginProvider, loginInfo.Login.ProviderKey, loginInfo.Login.LoginProvider));
                        if (linkResult.Succeeded == false)
                        {
                            ViewData.SetExternalSignInError(linkResult.Errors.Select(x => x.Description).ToList());

                            //If this fails, we should really delete the user since it will be in an inconsistent state!
                            var deleteResult = await UserManager.DeleteAsync(autoLinkUser);
                            if (deleteResult.Succeeded == false)
                            {
                                //DOH! ... this isn't good, combine all errors to be shown
                                ViewData.SetExternalSignInError(linkResult.Errors.Concat(deleteResult.Errors).Select(x => x.Description).ToList());
                            }
                        }
                        else
                        {
                            //sign in
                            await SignInManager.SignInAsync(autoLinkUser, isPersistent: false, rememberBrowser: false);
                        }
                    }
                }

            }
            return true;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return Redirect("/");
        }

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri, string userId = null)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            private string LoginProvider { get; set; }
            private string RedirectUri { get; set; }
            private string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                //Ensure the forms auth module doesn't do a redirect!
                context.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;

                var owinCtx = context.HttpContext.GetOwinContext();

                //First, see if a custom challenge result callback is specified for the provider
                // and use it instead of the default if one is supplied.
                var loginProvider = owinCtx.Authentication
                    .GetExternalAuthenticationTypes()
                    .FirstOrDefault(p => p.AuthenticationType == LoginProvider);
                if (loginProvider != null)
                {
                    var providerChallengeResult = loginProvider.GetSignInChallengeResult(owinCtx);
                    if (providerChallengeResult != null)
                    {
                        owinCtx.Authentication.Challenge(providerChallengeResult, LoginProvider);
                        return;
                    }
                }

                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri.EnsureEndsWith('/') };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                owinCtx.Authentication.Challenge(properties, LoginProvider);
            }
        }
    }
}
