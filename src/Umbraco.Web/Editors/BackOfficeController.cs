using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Microsoft.AspNetCore.Identity;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Core.Services;
using Umbraco.Web.Features;
using Umbraco.Web.Models.Identity;
using Umbraco.Web.Security;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.WebAssets;
using Umbraco.Web.Trees;
using Umbraco.Web.WebAssets;

namespace Umbraco.Web.Editors
{

    /// <summary>
    /// Represents a controller user to render out the default back office view and JS results.
    /// </summary>
    [UmbracoRequireHttps]
    [DisableBrowserCache]
    public class BackOfficeController : UmbracoController
    {
        private readonly UmbracoFeatures _features;
        private readonly IRuntimeState _runtimeState;
        private BackOfficeUserManager<BackOfficeIdentityUser> _userManager;
        private BackOfficeSignInManager _signInManager;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IGridConfig _gridConfig;
        private readonly IContentSettings _contentSettings;
        private readonly TreeCollection _treeCollection;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly ISecuritySettings _securitySettings;
        private readonly IRuntimeMinifier _runtimeMinifier;

        public BackOfficeController(
            UmbracoFeatures features,
            IGlobalSettings globalSettings,
            IUmbracoContextAccessor umbracoContextAccessor,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IRuntimeState runtimeState,
            IUmbracoVersion umbracoVersion,
            IGridConfig gridConfig,
            IContentSettings contentSettings,
            TreeCollection treeCollection,
            IHostingEnvironment hostingEnvironment,
            IHttpContextAccessor httpContextAccessor,
            IRuntimeSettings settings,
            ISecuritySettings securitySettings,
            IRuntimeMinifier runtimeMinifier)
            : base(globalSettings, umbracoContextAccessor, services, appCaches, profilingLogger)

        {
            _features = features;
            _runtimeState = runtimeState;
            _umbracoVersion = umbracoVersion;
            _gridConfig = gridConfig ?? throw new ArgumentNullException(nameof(gridConfig));
            _contentSettings = contentSettings ?? throw new ArgumentNullException(nameof(contentSettings));
            _treeCollection = treeCollection ?? throw new ArgumentNullException(nameof(treeCollection));
            _hostingEnvironment = hostingEnvironment;
            _runtimeSettings = settings;
            _securitySettings = securitySettings;
            _runtimeMinifier = runtimeMinifier;
        }

        protected BackOfficeSignInManager SignInManager => _signInManager ?? (_signInManager = OwinContext.GetBackOfficeSignInManager());

        protected BackOfficeUserManager<BackOfficeIdentityUser> UserManager => _userManager ?? (_userManager = OwinContext.GetBackOfficeUserManager());

        protected IAuthenticationManager AuthenticationManager => OwinContext.Authentication;

        /// <summary>
        /// Render the default view
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Default()
        {
            return await RenderDefaultOrProcessExternalLoginAsync(
                () =>
                    View(GlobalSettings.GetBackOfficePath(_hostingEnvironment).EnsureEndsWith('/') + "Views/Default.cshtml", new BackOfficeModel(_features, GlobalSettings, _umbracoVersion, _contentSettings, _treeCollection, _hostingEnvironment, _runtimeSettings, _securitySettings)),
                () =>
                    View(GlobalSettings.GetBackOfficePath(_hostingEnvironment).EnsureEndsWith('/') + "Views/Default.cshtml", new BackOfficeModel(_features, GlobalSettings, _umbracoVersion, _contentSettings, _treeCollection, _hostingEnvironment, _runtimeSettings, _securitySettings))
                    );
        }

        [HttpGet]
        public async Task<ActionResult> VerifyInvite(string invite)
        {
            //if you are hitting VerifyInvite, you're already signed in as a different user, and the token is invalid
            //you'll exit on one of the return RedirectToAction("Default") but you're still logged in so you just get
            //dumped at the default admin view with no detail
            if(Security.IsAuthenticated())
            {
                AuthenticationManager.SignOut(
                    Core.Constants.Security.BackOfficeAuthenticationType,
                    Core.Constants.Security.BackOfficeExternalAuthenticationType);
            }

            if (invite == null)
            {
                Logger.Warn<BackOfficeController>("VerifyUser endpoint reached with invalid token: NULL");
                return RedirectToAction("Default");
            }

            var parts = Server.UrlDecode(invite).Split('|');

            if (parts.Length != 2)
            {
                Logger.Warn<BackOfficeController>("VerifyUser endpoint reached with invalid token: {Invite}", invite);
                return RedirectToAction("Default");
            }

            var token = parts[1];

            var decoded = token.FromUrlBase64();
            if (decoded.IsNullOrWhiteSpace())
            {
                Logger.Warn<BackOfficeController>("VerifyUser endpoint reached with invalid token: {Invite}", invite);
                return RedirectToAction("Default");
            }

            var id = parts[0];

            var identityUser = await UserManager.FindByIdAsync(id);
            if (identityUser == null)
            {
                Logger.Warn<BackOfficeController>("VerifyUser endpoint reached with non existing user: {UserId}", id);
                return RedirectToAction("Default");
            }

            var result = await UserManager.ConfirmEmailAsync(identityUser, decoded);

            if (result.Succeeded == false)
            {
                Logger.Warn<BackOfficeController>("Could not verify email, Error: {Errors}, Token: {Invite}", result.Errors.ToErrorMessage(), invite);
                return new RedirectResult(Url.Action("Default") + "#/login/false?invite=3");
            }

            //sign the user in
            DateTime? previousLastLoginDate = identityUser.LastLoginDateUtc;
            await SignInManager.SignInAsync(identityUser, false, false);
            //reset the lastlogindate back to previous as the user hasn't actually logged in, to add a flag or similar to SignInManager would be a breaking change
            identityUser.LastLoginDateUtc = previousLastLoginDate;
            await UserManager.UpdateAsync(identityUser);

            return new RedirectResult(Url.Action("Default") + "#/login/false?invite=1");
        }

        /// <summary>
        /// This Action is used by the installer when an upgrade is detected but the admin user is not logged in. We need to
        /// ensure the user is authenticated before the install takes place so we redirect here to show the standard login screen.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [StatusCodeResult(System.Net.HttpStatusCode.ServiceUnavailable)]
        public async Task<ActionResult> AuthorizeUpgrade()
        {
            return await RenderDefaultOrProcessExternalLoginAsync(
                //The default view to render when there is no external login info or errors
                () => View(GlobalSettings.GetBackOfficePath(_hostingEnvironment).EnsureEndsWith('/') + "Views/AuthorizeUpgrade.cshtml", new BackOfficeModel(_features, GlobalSettings, _umbracoVersion, _contentSettings, _treeCollection, _hostingEnvironment, _runtimeSettings, _securitySettings)),
                //The ActionResult to perform if external login is successful
                () => Redirect("/"));
        }

        /// <summary>
        /// Get the json localized text for a given culture or the culture for the current user
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <remarks>Migrated already to .Net Core</remarks>
        [HttpGet]
        public JsonNetResult LocalizedText(string culture = null)
        {
            var cultureInfo = string.IsNullOrWhiteSpace(culture)
                //if the user is logged in, get their culture, otherwise default to 'en'
                ? Security.IsAuthenticated()
                    //current culture is set at the very beginning of each request
                    ? Thread.CurrentThread.CurrentCulture
                    : CultureInfo.GetCultureInfo(GlobalSettings.DefaultUILanguage)
                : CultureInfo.GetCultureInfo(culture);

            var allValues = Services.TextService.GetAllStoredValues(cultureInfo);
            var pathedValues = allValues.Select(kv =>
            {
                var slashIndex = kv.Key.IndexOf('/');
                var areaAlias = kv.Key.Substring(0, slashIndex);
                var valueAlias = kv.Key.Substring(slashIndex+1);
                return new
                {
                    areaAlias,
                    valueAlias,
                    value = kv.Value
                };
            });

            Dictionary<string, Dictionary<string, string>> nestedDictionary = pathedValues
                .GroupBy(pv => pv.areaAlias)
                .ToDictionary(pv => pv.Key, pv =>
                    pv.ToDictionary(pve => pve.valueAlias, pve => pve.value));

            return new JsonNetResult { Data = nestedDictionary, Formatting = Formatting.None };
        }

        /// <summary>
        /// Returns the JavaScript main file including all references found in manifests
        /// </summary>
        /// <returns></returns>
        [MinifyJavaScriptResult(Order = 0)]
        [OutputCache(Order = 1, VaryByParam = "none", Location = OutputCacheLocation.Server, Duration = 5000)]
        public async Task<JavaScriptResult> Application()
        {
            var result = await _runtimeMinifier.GetScriptForLoadingBackOfficeAsync(GlobalSettings, _hostingEnvironment);

            return JavaScript(result);
        }

        /// Migrated already to .Net Core
        [UmbracoAuthorize(Order = 0)]
        [HttpGet]
        public JsonNetResult GetGridConfig()
        {
            return new JsonNetResult { Data = _gridConfig.EditorsConfig.Editors, Formatting = Formatting.None };
        }



        /// <summary>
        /// Returns the JavaScript object representing the static server variables javascript object
        /// </summary>
        /// <returns></returns>
        [UmbracoAuthorize(Order = 0)]
        [MinifyJavaScriptResult(Order = 1)]
        public JavaScriptResult ServerVariables()
        {
            var serverVars = new BackOfficeServerVariables(Url, _runtimeState, _features, GlobalSettings, _umbracoVersion, _contentSettings, _treeCollection, _hostingEnvironment, _runtimeSettings, _securitySettings, _runtimeMinifier);

            //cache the result if debugging is disabled
            var result = _hostingEnvironment.IsDebugMode
                ? ServerVariablesParser.Parse(serverVars.GetServerVariables())
                : AppCaches.RuntimeCache.GetCacheItem<string>(
                    typeof(BackOfficeController) + "ServerVariables",
                    () => ServerVariablesParser.Parse(serverVars.GetServerVariables()),
                    new TimeSpan(0, 10, 0));

            return JavaScript(result);
        }



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

        [UmbracoAuthorize]
        [HttpPost]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider,
                Url.Action("ExternalLinkLoginCallback", "BackOffice"),
                User.Identity.GetUserId());
        }

        [HttpGet]
        public async Task<ActionResult> ValidatePasswordResetCode([Bind(Prefix = "u")]int userId, [Bind(Prefix = "r")]string resetCode)
        {
            var user = await UserManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                var result = await UserManager.VerifyUserTokenAsync(user, "ResetPassword", "ResetPassword", resetCode);
                if (result)
                {
                    //Add a flag and redirect for it to be displayed
                    TempData[ViewDataExtensions.TokenPasswordResetCode] = new ValidatePasswordResetCodeModel { UserId = userId, ResetCode = resetCode };
                    return RedirectToLocal(Url.Action("Default", "BackOffice"));
                }
            }

            //Add error and redirect for it to be displayed
            TempData[ViewDataExtensions.TokenPasswordResetCode] = new[] { Services.TextService.Localize("login/resetCodeExpired") };
            return RedirectToLocal(Url.Action("Default", "BackOffice"));
        }

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

            ViewData.SetUmbracoPath(GlobalSettings.GetUmbracoMvcArea(_hostingEnvironment));

            //check if there is the TempData with the any token name specified, if so, assign to view bag and render the view
            if (ViewData.FromTempData(TempData, ViewDataExtensions.TokenExternalSignInError) ||
                ViewData.FromTempData(TempData, ViewDataExtensions.TokenPasswordResetCode))
                return defaultResponse();

            //First check if there's external login info, if there's not proceed as normal
            var loginInfo = await OwinContext.Authentication.GetExternalLoginInfoAsync(
                Constants.Security.BackOfficeExternalAuthenticationType);

            if (loginInfo == null || loginInfo.ExternalIdentity.IsAuthenticated == false)
            {
                return defaultResponse();
            }

            //we're just logging in with an external source, not linking accounts
            return await ExternalSignInAsync(loginInfo, externalSignInResponse);
        }

        private async Task<ActionResult> ExternalSignInAsync(ExternalLoginInfo loginInfo, Func<ActionResult> response)
        {
            if (loginInfo == null) throw new ArgumentNullException("loginInfo");
            if (response == null) throw new ArgumentNullException("response");
            ExternalSignInAutoLinkOptions autoLinkOptions = null;

            //Here we can check if the provider associated with the request has been configured to allow
            // new users (auto-linked external accounts). This would never be used with public providers such as
            // Google, unless you for some reason wanted anybody to be able to access the backend if they have a Google account
            // .... not likely!
            var authType = OwinContext.Authentication.GetExternalAuthenticationTypes().FirstOrDefault(x => x.AuthenticationType == loginInfo.Login.LoginProvider);
            if (authType == null)
            {
                Logger.Warn<BackOfficeController>("Could not find external authentication provider registered: {LoginProvider}", loginInfo.Login.LoginProvider);
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
                        Logger.Warn<BackOfficeController>("The AutoLinkOptions of the external authentication provider '{LoginProvider}' have refused the login based on the OnExternalLogin method. Affected user id: '{UserId}'", loginInfo.Login.LoginProvider, user.Id);
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

            if (autoLinkOptions.ShouldAutoLinkExternalAccount(UmbracoContext, loginInfo) == false)
                return true;

            //we are allowing auto-linking/creating of local accounts
            if (loginInfo.Email.IsNullOrWhiteSpace())
            {
                ViewData.SetExternalSignInError(new[] { "The requested provider (" + loginInfo.Login.LoginProvider + ") has not provided an email address, the account cannot be linked." });
            }
            else
            {
                //Now we need to perform the auto-link, so first we need to lookup/create a user with the email address
                var foundByEmail = Services.UserService.GetByEmail(loginInfo.Email);
                if (foundByEmail != null)
                {
                    ViewData.SetExternalSignInError(new[] { "A user with this email address already exists locally. You will need to login locally to Umbraco and link this external provider: " + loginInfo.Login.LoginProvider });
                }
                else
                {
                    if (loginInfo.Email.IsNullOrWhiteSpace()) throw new InvalidOperationException("The Email value cannot be null");
                    if (loginInfo.ExternalIdentity.Name.IsNullOrWhiteSpace()) throw new InvalidOperationException("The Name value cannot be null");

                    var groups = Services.UserService.GetUserGroupsByAlias(autoLinkOptions.GetDefaultUserGroups(UmbracoContext, loginInfo));

                    var autoLinkUser = BackOfficeIdentityUser.CreateNew(
                        GlobalSettings,
                        loginInfo.Email,
                        loginInfo.Email,
                        autoLinkOptions.GetDefaultCulture(UmbracoContext, loginInfo));
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
