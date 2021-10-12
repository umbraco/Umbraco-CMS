using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Features;
using Umbraco.Web.JavaScript;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.Security;
using Constants = Umbraco.Core.Constants;
using JArray = Newtonsoft.Json.Linq.JArray;

namespace Umbraco.Web.Editors
{

    /// <summary>
    /// Represents a controller user to render out the default back office view and JS results.
    /// </summary>
    [UmbracoRequireHttps]
    [DisableBrowserCache]
    public class BackOfficeController : UmbracoController
    {
        private readonly ManifestParser _manifestParser;
        private readonly UmbracoFeatures _features;
        private readonly IRuntimeState _runtimeState;
        private BackOfficeUserManager<BackOfficeIdentityUser> _userManager;
        private BackOfficeSignInManager _signInManager;

        public BackOfficeController(
            ManifestParser manifestParser,
            UmbracoFeatures features,
            IGlobalSettings globalSettings,
            IUmbracoContextAccessor umbracoContextAccessor,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IRuntimeState runtimeState,
            UmbracoHelper umbracoHelper)
            : base(globalSettings, umbracoContextAccessor, services, appCaches, profilingLogger, umbracoHelper)
        {
            _manifestParser = manifestParser;
            _features = features;
            _runtimeState = runtimeState;
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
            var backofficeModel = new BackOfficeModel(_features, GlobalSettings);
            return await RenderDefaultOrProcessExternalLoginAsync(
                () => View(GlobalSettings.Path.EnsureEndsWith('/') + "Views/Default.cshtml", backofficeModel),
                () => View(GlobalSettings.Path.EnsureEndsWith('/') + "Views/Default.cshtml", backofficeModel));
        }

        [HttpGet]
        public async Task<ActionResult> VerifyInvite(string invite)
        {
            //if you are hitting VerifyInvite, you're already signed in as a different user, and the token is invalid
            //you'll exit on one of the return RedirectToAction("Default") but you're still logged in so you just get
            //dumped at the default admin view with no detail
            if (Security.IsAuthenticated())
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

            var parts = Server.UrlDecode(invite).Split(Constants.CharArrays.VerticalTab);

            if (parts.Length != 2)
            {
                Logger.Warn<BackOfficeController, string>("VerifyUser endpoint reached with invalid token: {Invite}", invite);
                return RedirectToAction("Default");
            }

            var token = parts[1];

            var decoded = token.FromUrlBase64();
            if (decoded.IsNullOrWhiteSpace())
            {
                Logger.Warn<BackOfficeController, string>("VerifyUser endpoint reached with invalid token: {Invite}", invite);
                return RedirectToAction("Default");
            }

            var id = parts[0];
            int intId;
            if (int.TryParse(id, out intId) == false)
            {
                Logger.Warn<BackOfficeController, string>("VerifyUser endpoint reached with invalid token: {Invite}", invite);
                return RedirectToAction("Default");
            }

            var identityUser = await UserManager.FindByIdAsync(intId);
            if (identityUser == null)
            {
                Logger.Warn<BackOfficeController, string>("VerifyUser endpoint reached with non existing user: {UserId}", id);
                return RedirectToAction("Default");
            }

            var result = await UserManager.ConfirmEmailAsync(intId, decoded);

            if (result.Succeeded == false)
            {
                Logger.Warn<BackOfficeController, string, string>("Could not verify email, Error: {Errors}, Token: {Invite}", string.Join(",", result.Errors), invite);
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
                () => View(GlobalSettings.Path.EnsureEndsWith('/') + "Views/AuthorizeUpgrade.cshtml", new BackOfficeModel(_features, GlobalSettings)),
                //The ActionResult to perform if external login is successful
                () => Redirect("/"));
        }

        /// <summary>
        /// Get the json localized text for a given culture or the culture for the current user
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
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

            // TODO: Remove this check in v9 when these interfaces merge
            if(Services.TextService is ILocalizedTextService2 localizedText2)
            {
                var nestedDictionary2 = localizedText2.GetAllStoredValuesByAreaAndAlias(cultureInfo);
                return new JsonNetResult { Data = nestedDictionary2, Formatting = Formatting.None };
            }

            var allValues = Services.TextService.GetAllStoredValues(cultureInfo);
            var pathedValues = allValues.Select(kv =>
            {
                var slashIndex = kv.Key.IndexOf('/');
                var areaAlias = kv.Key.Substring(0, slashIndex);
                var valueAlias = kv.Key.Substring(slashIndex + 1);
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

            return new JsonNetResult(JsonNetResult.DefaultJsonSerializerSettings) { Data = nestedDictionary, Formatting = Formatting.None };
        }

        /// <summary>
        /// Returns the JavaScript main file including all references found in manifests
        /// </summary>
        /// <returns></returns>
        [MinifyJavaScriptResult(Order = 0)]
        [OutputCache(Order = 1, VaryByParam = "none", Location = OutputCacheLocation.Server, Duration = 5000)]
        public JavaScriptResult Application()
        {
            var initJs = new JsInitialization(_manifestParser);
            var initCss = new CssInitialization(_manifestParser);

            var files = initJs.OptimizeBackOfficeScriptFiles(HttpContext, JsInitialization.GetDefaultInitialization());
            var result = JsInitialization.GetJavascriptInitialization(HttpContext, files, "umbraco");
            result += initCss.GetStylesheetInitialization(HttpContext);

            return JavaScript(result);
        }

        /// <summary>
        /// Returns a js array of all of the manifest assets
        /// </summary>
        /// <returns></returns>
        [UmbracoAuthorize(Order = 0)]
        [HttpGet]
        public JsonNetResult GetManifestAssetList()
        {
            JArray GetAssetList()
            {
                var initJs = new JsInitialization(_manifestParser);
                var initCss = new CssInitialization(_manifestParser);
                var assets = new List<string>();
                assets.AddRange(initJs.OptimizeBackOfficeScriptFiles(HttpContext, Enumerable.Empty<string>()));
                assets.AddRange(initCss.GetStylesheetFiles(HttpContext));
                return new JArray(assets);
            }

            //cache the result if debugging is disabled
            var result = HttpContext.IsDebuggingEnabled
                ? GetAssetList()
                : AppCaches.RuntimeCache.GetCacheItem<JArray>(
                    "Umbraco.Web.Editors.BackOfficeController.GetManifestAssetList",
                    GetAssetList,
                    new TimeSpan(0, 2, 0));

            return new JsonNetResult(JsonNetResult.DefaultJsonSerializerSettings) { Data = result, Formatting = Formatting.None };
        }

        [UmbracoAuthorize(Order = 0)]
        [HttpGet]
        public JsonNetResult GetGridConfig()
        {
            var gridConfig = Current.Configs.Grids();
            return new JsonNetResult(JsonNetResult.DefaultJsonSerializerSettings) { Data = gridConfig.EditorsConfig.Editors, Formatting = Formatting.None };
        }



        /// <summary>
        /// Returns the JavaScript object representing the static server variables javascript object
        /// </summary>
        /// <returns></returns>
        [UmbracoAuthorize(Order = 0)]
        [MinifyJavaScriptResult(Order = 1)]
        public JavaScriptResult ServerVariables()
        {
            var serverVars = new BackOfficeServerVariables(Url, _runtimeState, _features, GlobalSettings);

            //cache the result if debugging is disabled
            var result = HttpContext.IsDebuggingEnabled
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
        public async Task<ActionResult> ValidatePasswordResetCode([Bind(Prefix = "u")] int userId, [Bind(Prefix = "r")] string resetCode)
        {
            var user = UserManager.FindById(userId);
            if (user != null)
            {
                var result = await UserManager.UserTokenProvider.ValidateAsync("ResetPassword", resetCode, UserManager, user);
                if (result)
                {
                    //Add a flag and redirect for it to be displayed
                    TempData[ViewDataExtensions.TokenPasswordResetCode] = new ValidatePasswordResetCodeModel { UserId = userId, ResetCode = resetCode };
                    return RedirectToLocal(Url.Action("Default", "BackOffice"));
                }
            }

            //Add error and redirect for it to be displayed
            TempData[ViewDataExtensions.TokenPasswordResetCode] = new[] { Services.TextService.Localize("login", "resetCodeExpired") };
            return RedirectToLocal(Url.Action("Default", "BackOffice"));
        }

        [UmbracoAuthorize]
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

            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId<int>(), loginInfo.Login);
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

            ViewData.SetUmbracoPath(GlobalSettings.GetUmbracoMvcArea());

            //check if there is the TempData or cookies with the any token name specified, if so, assign to view bag and render the view
            if (ViewData.FromBase64CookieData<BackOfficeExternalLoginProviderErrors>(HttpContext, ViewDataExtensions.TokenExternalSignInError) ||
                ViewData.FromTempData(TempData, ViewDataExtensions.TokenExternalSignInError) ||
                ViewData.FromTempData(TempData, ViewDataExtensions.TokenPasswordResetCode))
                return defaultResponse();

            //First check if there's external login info, if there's not proceed as normal
            var loginInfo = await OwinContext.Authentication.GetExternalLoginInfoAsync(
                Constants.Security.BackOfficeExternalAuthenticationType);

            if (loginInfo == null || loginInfo.ExternalIdentity.IsAuthenticated == false)
            {

                // if the user is not logged in, check if there's any auto login redirects specified
                if (UmbracoContext.Security.ValidateCurrentUser(false) != ValidateRequestAttempt.Success)
                {
                    var oauthRedirectAuthProvider = OwinContext.Authentication.GetAutoLoginProvider();
                    if (!oauthRedirectAuthProvider.IsNullOrWhiteSpace())
                    {
                        return ExternalLogin(oauthRedirectAuthProvider);
                    }
                }

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

            // Here we can check if the provider associated with the request has been configured to allow
            // new users (auto-linked external accounts). This would never be used with public providers such as
            // Google, unless you for some reason wanted anybody to be able to access the backend if they have a Google account
            // .... not likely!
            var authType = OwinContext.Authentication.GetExternalAuthenticationTypes().FirstOrDefault(x => x.AuthenticationType == loginInfo.Login.LoginProvider);
            if (authType == null)
            {
                Logger.Warn<BackOfficeController, string>("Could not find external authentication provider registered: {LoginProvider}", loginInfo.Login.LoginProvider);
            }
            else
            {
                autoLinkOptions = authType.GetExternalSignInAutoLinkOptions();
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                var shouldSignIn = true;
                if (autoLinkOptions != null && autoLinkOptions.OnExternalLogin != null)
                {
                    shouldSignIn = autoLinkOptions.OnExternalLogin(user, loginInfo);
                    if (shouldSignIn == false)
                    {
                        Logger.Warn<BackOfficeController, string,int>("The AutoLinkOptions of the external authentication provider '{LoginProvider}' have refused the login based on the OnExternalLogin method. Affected user id: '{UserId}'", loginInfo.Login.LoginProvider, user.Id);
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
                    ViewData.SetExternalSignInProviderErrors(
                        new BackOfficeExternalLoginProviderErrors(
                            loginInfo.Login.LoginProvider,
                            new[] { "The requested provider (" + loginInfo.Login.LoginProvider + ") has not been linked to an account" }));
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
                ViewData.SetExternalSignInProviderErrors(
                    new BackOfficeExternalLoginProviderErrors(
                        loginInfo.Login.LoginProvider,
                        new[] { "The requested provider (" + loginInfo.Login.LoginProvider + ") has not provided an email address, the account cannot be linked." }));
            }
            else
            {
                //Now we need to perform the auto-link, so first we need to lookup/create a user with the email address
                var autoLinkUser = UserManager.FindByEmail(loginInfo.Email);
                if (autoLinkUser != null)
                {
                    try
                    {
                        //call the callback if one is assigned
                        autoLinkOptions.OnAutoLinking?.Invoke(autoLinkUser, loginInfo);
                    }
                    catch (Exception ex)
                    {
                        var msg = "Could not link login provider " + loginInfo.Login.LoginProvider + ".";
                        Logger.Error<BackOfficeController>(ex, msg);
                        ViewData.SetExternalSignInProviderErrors(
                            new BackOfficeExternalLoginProviderErrors(
                                loginInfo.Login.LoginProvider,
                                new[] { msg + " " + ex.Message }));
                        return true;
                    }

                    await LinkUser(autoLinkUser, loginInfo);
                }
                else
                {
                    if (loginInfo.Email.IsNullOrWhiteSpace()) throw new InvalidOperationException("The Email value cannot be null");
                    if (loginInfo.ExternalIdentity.Name.IsNullOrWhiteSpace()) throw new InvalidOperationException("The Name value cannot be null");

                    var groups = Services.UserService.GetUserGroupsByAlias(autoLinkOptions.GetDefaultUserGroups(UmbracoContext, loginInfo));

                    autoLinkUser = BackOfficeIdentityUser.CreateNew(
                        loginInfo.Email,
                        loginInfo.Email,
                        autoLinkOptions.GetDefaultCulture(UmbracoContext, loginInfo));
                    autoLinkUser.Name = loginInfo.ExternalIdentity.Name;
                    foreach (var userGroup in groups)
                    {
                        autoLinkUser.AddRole(userGroup.Alias);
                    }

                    //call the callback if one is assigned
                    try
                    {
                        autoLinkOptions.OnAutoLinking?.Invoke(autoLinkUser, loginInfo);
                    }
                    catch (Exception ex)
                    {
                        var msg = "Could not link login provider " + loginInfo.Login.LoginProvider + ".";
                        Logger.Error<BackOfficeController>(ex, msg);
                        ViewData.SetExternalSignInProviderErrors(
                            new BackOfficeExternalLoginProviderErrors(
                                loginInfo.Login.LoginProvider,
                                new[] { msg + " " + ex.Message }));
                        return true;
                    }

                    var userCreationResult = await UserManager.CreateAsync(autoLinkUser);

                    if (userCreationResult.Succeeded == false)
                    {
                        ViewData.SetExternalSignInProviderErrors(
                            new BackOfficeExternalLoginProviderErrors(
                                loginInfo.Login.LoginProvider,
                                userCreationResult.Errors));
                    }
                    else
                    {
                        await LinkUser(autoLinkUser, loginInfo);
                    }
                }

            }
            return true;
        }

        private async Task LinkUser(BackOfficeIdentityUser autoLinkUser, ExternalLoginInfo loginInfo)
        {
            var existingLogins = await UserManager.GetLoginsAsync(autoLinkUser.Id);
            var exists = existingLogins.FirstOrDefault(x => x.LoginProvider == loginInfo.Login.LoginProvider && x.ProviderKey == loginInfo.Login.ProviderKey);

            // if it already exists (perhaps it was added in the AutoLink callbak) then we just continue
            if (exists != null)
            {
                //sign in
                await SignInManager.SignInAsync(autoLinkUser, isPersistent: false, rememberBrowser: false);
                return;
            }

            var linkResult = await UserManager.AddLoginAsync(autoLinkUser.Id, loginInfo.Login);
            if (linkResult.Succeeded)
            {
                //we're good! sign in
                await SignInManager.SignInAsync(autoLinkUser, isPersistent: false, rememberBrowser: false);
                return;
            }

            ViewData.SetExternalSignInProviderErrors(
                   new BackOfficeExternalLoginProviderErrors(
                       loginInfo.Login.LoginProvider,
                       linkResult.Errors));

            //If this fails, we should really delete the user since it will be in an inconsistent state!
            var deleteResult = await UserManager.DeleteAsync(autoLinkUser);
            if (!deleteResult.Succeeded)
            {
                //DOH! ... this isn't good, combine all errors to be shown
                ViewData.SetExternalSignInProviderErrors(
                    new BackOfficeExternalLoginProviderErrors(
                        loginInfo.Login.LoginProvider,
                        linkResult.Errors.Concat(deleteResult.Errors)));
            }
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
        public const string XsrfKey = "XsrfId";

    }

}
