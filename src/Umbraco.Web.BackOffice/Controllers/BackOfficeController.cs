using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Security;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.WebAssets;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.ActionResults;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Common.Security;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebAssets;
using Constants = Umbraco.Core.Constants;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Controllers
{
    //[UmbracoRequireHttps] //TODO Reintroduce
    [PluginController(Constants.Web.Mvc.BackOfficeArea)]
    public class BackOfficeController : UmbracoController
    {
        private readonly IBackOfficeUserManager _userManager;
        private readonly IRuntimeMinifier _runtimeMinifier;
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILocalizedTextService _textService;
        private readonly IGridConfig _gridConfig;
        private readonly BackOfficeServerVariables _backOfficeServerVariables;
        private readonly AppCaches _appCaches;
        private readonly BackOfficeSignInManager _signInManager;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly ILogger<BackOfficeController> _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IBackOfficeExternalLoginProviders _externalLogins;

        public BackOfficeController(
            IBackOfficeUserManager userManager,
            IRuntimeMinifier runtimeMinifier,
            IOptions<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment,
            ILocalizedTextService textService,
            IGridConfig gridConfig,
            BackOfficeServerVariables backOfficeServerVariables,
            AppCaches appCaches,
            BackOfficeSignInManager signInManager,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            ILogger<BackOfficeController> logger,
            IJsonSerializer jsonSerializer,
            IBackOfficeExternalLoginProviders externalLogins)
        {
            _userManager = userManager;
            _runtimeMinifier = runtimeMinifier;
            _globalSettings = globalSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _textService = textService;
            _gridConfig = gridConfig ?? throw new ArgumentNullException(nameof(gridConfig));
            _backOfficeServerVariables = backOfficeServerVariables;
            _appCaches = appCaches;
            _signInManager = signInManager;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            _externalLogins = externalLogins;
        }

        [HttpGet]
        public async Task<IActionResult> Default()
        {
            var viewPath = Path.Combine(_globalSettings.UmbracoPath , Constants.Web.Mvc.BackOfficeArea, nameof(Default) + ".cshtml")
                .Replace("\\", "/"); // convert to forward slashes since it's a virtual path

            return await RenderDefaultOrProcessExternalLoginAsync(
                () => View(viewPath),
                () => View(viewPath));
        }

        [HttpGet]
        public async Task<IActionResult> VerifyInvite(string invite)
        {
            //if you are hitting VerifyInvite, you're already signed in as a different user, and the token is invalid
            //you'll exit on one of the return RedirectToAction(nameof(Default)) but you're still logged in so you just get
            //dumped at the default admin view with no detail
            if (_backofficeSecurityAccessor.BackOfficeSecurity.IsAuthenticated())
            {
                await _signInManager.SignOutAsync();
            }

            if (invite == null)
            {
                _logger.LogWarning("VerifyUser endpoint reached with invalid token: NULL");
                return RedirectToAction(nameof(Default));
            }

            var parts = System.Net.WebUtility.UrlDecode(invite).Split('|');

            if (parts.Length != 2)
            {
                _logger.LogWarning("VerifyUser endpoint reached with invalid token: {Invite}", invite);
                return RedirectToAction(nameof(Default));
            }

            var token = parts[1];

            var decoded = token.FromUrlBase64();
            if (decoded.IsNullOrWhiteSpace())
            {
                _logger.LogWarning("VerifyUser endpoint reached with invalid token: {Invite}", invite);
                return RedirectToAction(nameof(Default));
            }

            var id = parts[0];

            var identityUser = await _userManager.FindByIdAsync(id);
            if (identityUser == null)
            {
                _logger.LogWarning("VerifyUser endpoint reached with non existing user: {UserId}", id);
                return RedirectToAction(nameof(Default));
            }

            var result = await _userManager.ConfirmEmailAsync(identityUser, decoded);

            if (result.Succeeded == false)
            {
                _logger.LogWarning("Could not verify email, Error: {Errors}, Token: {Invite}", result.Errors.ToErrorMessage(), invite);
                return new RedirectResult(Url.Action(nameof(Default)) + "#/login/false?invite=3");
            }

            //sign the user in
            DateTime? previousLastLoginDate = identityUser.LastLoginDateUtc;
            await _signInManager.SignInAsync(identityUser, false);
            //reset the lastlogindate back to previous as the user hasn't actually logged in, to add a flag or similar to BackOfficeSignInManager would be a breaking change
            identityUser.LastLoginDateUtc = previousLastLoginDate;
            await _userManager.UpdateAsync(identityUser);

            return new RedirectResult(Url.Action(nameof(Default)) + "#/login/false?invite=1");
        }

        /// <summary>
        /// This Action is used by the installer when an upgrade is detected but the admin user is not logged in. We need to
        /// ensure the user is authenticated before the install takes place so we redirect here to show the standard login screen.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [StatusCodeResult(System.Net.HttpStatusCode.ServiceUnavailable)]
        public async Task<IActionResult> AuthorizeUpgrade()
        {
            var viewPath = Path.Combine(_globalSettings.UmbracoPath, Umbraco.Core.Constants.Web.Mvc.BackOfficeArea, nameof(AuthorizeUpgrade) + ".cshtml");
            return await RenderDefaultOrProcessExternalLoginAsync(
                //The default view to render when there is no external login info or errors
                () => View(viewPath),
                //The IActionResult to perform if external login is successful
                () => Redirect("/"));
        }

        /// <summary>
        /// Returns the JavaScript main file including all references found in manifests
        /// </summary>
        /// <returns></returns>
        [MinifyJavaScriptResult(Order = 0)]
        [HttpGet]
        public async Task<IActionResult> Application()
        {
            var result = await _runtimeMinifier.GetScriptForLoadingBackOfficeAsync(_globalSettings, _hostingEnvironment);

            return new JavaScriptResult(result);
        }

        /// <summary>
        /// Get the json localized text for a given culture or the culture for the current user
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        [HttpGet]
        public Dictionary<string, Dictionary<string, string>> LocalizedText(string culture = null)
        {
            var isAuthenticated = _backofficeSecurityAccessor.BackOfficeSecurity.IsAuthenticated();

            var cultureInfo = string.IsNullOrWhiteSpace(culture)
                //if the user is logged in, get their culture, otherwise default to 'en'
                ? isAuthenticated
                    //current culture is set at the very beginning of each request
                    ? Thread.CurrentThread.CurrentCulture
                    : CultureInfo.GetCultureInfo(_globalSettings.DefaultUILanguage)
                : CultureInfo.GetCultureInfo(culture);

            var allValues = _textService.GetAllStoredValues(cultureInfo);
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

            var nestedDictionary = pathedValues
                .GroupBy(pv => pv.areaAlias)
                .ToDictionary(pv => pv.Key, pv =>
                    pv.ToDictionary(pve => pve.valueAlias, pve => pve.value));

            return nestedDictionary;
        }

        [UmbracoAuthorize(Order = 0)]
        [HttpGet]
        public IEnumerable<IGridEditorConfig> GetGridConfig()
        {
            return _gridConfig.EditorsConfig.Editors;
        }

        /// <summary>
        /// Returns the JavaScript object representing the static server variables javascript object
        /// </summary>
        /// <returns></returns>
        [UmbracoAuthorize(Order = 0)]
        [MinifyJavaScriptResult(Order = 1)]
        public async Task<JavaScriptResult> ServerVariables()
        {
            //cache the result if debugging is disabled
            var serverVars = ServerVariablesParser.Parse(await _backOfficeServerVariables.GetServerVariablesAsync());
            var result = _hostingEnvironment.IsDebugMode
                ? serverVars
                : _appCaches.RuntimeCache.GetCacheItem<string>(
                    typeof(BackOfficeController) + "ServerVariables",
                    () => serverVars,
                    new TimeSpan(0, 10, 0));

            return new JavaScriptResult(result);
        }

        [HttpPost]
        public ActionResult ExternalLogin(string provider, string redirectUrl = null)
        {
            if (redirectUrl == null)
            {
                redirectUrl = Url.Action(nameof(Default), this.GetControllerName());
            }

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            // TODO: I believe we will have to fill in our own XsrfKey like we use to do since I think
            // we validate against that key?
            // see https://github.com/umbraco/Umbraco-CMS/blob/v8/contrib/src/Umbraco.Web/Editors/ChallengeResult.cs#L48
            return Challenge(properties, provider);
        }

        /// <summary>
        /// Called when a user links an external login provider in the back office
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        [UmbracoAuthorize]
        [HttpPost]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action(nameof(ExternalLinkLoginCallback), this.GetControllerName());
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, User.Identity.GetUserId());
            // TODO: I believe we will have to fill in our own XsrfKey like we use to do since I think
            // we validate against that key?
            // see https://github.com/umbraco/Umbraco-CMS/blob/v8/contrib/src/Umbraco.Web/Editors/ChallengeResult.cs#L48
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ValidatePasswordResetCode([Bind(Prefix = "u")]int userId, [Bind(Prefix = "r")]string resetCode)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                var result = await _userManager.VerifyUserTokenAsync(user, "Default", "ResetPassword", resetCode);
                if (result)
                {
                    //Add a flag and redirect for it to be displayed
                    TempData[ViewDataExtensions.TokenPasswordResetCode] =  _jsonSerializer.Serialize(new ValidatePasswordResetCodeModel { UserId = userId, ResetCode = resetCode });
                    return RedirectToLocal(Url.Action(nameof(Default), this.GetControllerName()));
                }
            }

            //Add error and redirect for it to be displayed
            TempData[ViewDataExtensions.TokenPasswordResetCode] = new[] { _textService.Localize("login/resetCodeExpired") };
            return RedirectToLocal(Url.Action(nameof(Default), this.GetControllerName()));
        }

        /// <summary>
        /// Callback path when the user initiates a link login request from the back office to the external provider from the <see cref="LinkLogin(string)"/> action
        /// </summary>
        [UmbracoAuthorize]
        [HttpGet]
        public async Task<IActionResult> ExternalLinkLoginCallback()
        {
            // TODO: Do we need/want to tell it an expected xsrf.
            // In v8 the xsrf used to be set to the user id which was verified manually, in this case I think we don't specify
            // the key and that is up to the underlying sign in manager to set so we'd just tell it to expect the user id,
            // the XSRF value used to be set in our ChallengeResult but now we don't have that so this needs to be set in the
            // BackOfficeController when we issue a Challenge, see TODO notes there.
            var loginInfo = await _signInManager.GetExternalLoginInfoAsync();

            if (loginInfo == null)
            {
                //Add error and redirect for it to be displayed
                TempData[ViewDataExtensions.TokenExternalSignInError] = new[] { "An error occurred, could not get external login info" };
                return RedirectToLocal(Url.Action(nameof(Default), this.GetControllerName()));
            }

            var user = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                // ... this should really not happen
                TempData[ViewDataExtensions.TokenExternalSignInError] = new[] { "Local user does not exist" };
                return RedirectToLocal(Url.Action(nameof(Default), this.GetControllerName()));
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
            if (addLoginResult.Succeeded)
            {
                // Update any authentication tokens if login succeeded
                // TODO: This is a new thing that we need to implement and because we can store data with the external login now, this is exactly
                // what this is for but we'll need to peek under the code here to figure out exactly what goes on.
                //await _signInManager.UpdateExternalAuthenticationTokensAsync(loginInfo);

                return RedirectToLocal(Url.Action(nameof(Default), this.GetControllerName()));
            }

            //Add errors and redirect for it to be displayed
            TempData[ViewDataExtensions.TokenExternalSignInError] = addLoginResult.Errors;
            return RedirectToLocal(Url.Action(nameof(Default), this.GetControllerName()));
        }

        /// <summary>
        /// Used by Default and AuthorizeUpgrade to render as per normal if there's no external login info,
        /// otherwise process the external login info.
        /// </summary>
        /// <returns></returns>
        private async Task<IActionResult> RenderDefaultOrProcessExternalLoginAsync(
            Func<IActionResult> defaultResponse,
            Func<IActionResult> externalSignInResponse)
        {
            if (defaultResponse is null) throw new ArgumentNullException(nameof(defaultResponse));
            if (externalSignInResponse is null) throw new ArgumentNullException(nameof(externalSignInResponse));

            ViewData.SetUmbracoPath(_globalSettings.GetUmbracoMvcArea(_hostingEnvironment));

            //check if there is the TempData or cookies with the any token name specified, if so, assign to view bag and render the view
            if (ViewData.FromBase64CookieData<BackOfficeExternalLoginProviderErrors>(HttpContext, ViewDataExtensions.TokenExternalSignInError, _jsonSerializer) ||
                ViewData.FromTempData(TempData, ViewDataExtensions.TokenExternalSignInError) ||
                ViewData.FromTempData(TempData, ViewDataExtensions.TokenPasswordResetCode))
                return defaultResponse();

            //First check if there's external login info, if there's not proceed as normal
            var loginInfo = await _signInManager.GetExternalLoginInfoAsync();

            if (loginInfo == null || loginInfo.Principal == null)
            {
                // if the user is not logged in, check if there's any auto login redirects specified
                if (!_backofficeSecurityAccessor.BackOfficeSecurity.ValidateCurrentUser())
                {
                    var oauthRedirectAuthProvider = _externalLogins.GetAutoLoginProvider();
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

        private async Task<IActionResult> ExternalSignInAsync(ExternalLoginInfo loginInfo, Func<IActionResult> response)
        {
            if (loginInfo == null) throw new ArgumentNullException(nameof(loginInfo));
            if (response == null) throw new ArgumentNullException(nameof(response));
            ExternalSignInAutoLinkOptions autoLinkOptions = null;

            var authType = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .FirstOrDefault(x => x.Name == loginInfo.LoginProvider);

            if (authType == null)
            {
                _logger.LogWarning("Could not find external authentication provider registered: {LoginProvider}", loginInfo.LoginProvider);
            }
            else
            {
                autoLinkOptions = _externalLogins.Get(authType.Name);
            }

            // Sign in the user with this external login provider if the user already has a login

            var user = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
            if (user != null)
            {
                var shouldSignIn = true;
                if (autoLinkOptions != null && autoLinkOptions.OnExternalLogin != null)
                {
                    shouldSignIn = autoLinkOptions.OnExternalLogin(user, loginInfo);
                    if (shouldSignIn == false)
                    {
                        _logger.LogWarning("The AutoLinkOptions of the external authentication provider '{LoginProvider}' have refused the login based on the OnExternalLogin method. Affected user id: '{UserId}'", loginInfo.LoginProvider, user.Id);
                    }
                }

                if (shouldSignIn)
                {
                    //sign in
                    await _signInManager.SignInAsync(user, false);
                }
            }
            else
            {
                if (await AutoLinkAndSignInExternalAccount(loginInfo, autoLinkOptions) == false)
                {
                    ViewData.SetExternalSignInProviderErrors(
                        new BackOfficeExternalLoginProviderErrors(
                            loginInfo.LoginProvider,
                            new[] { "The requested provider (" + loginInfo.LoginProvider + ") has not been linked to an account" }));
                }

                //Remove the cookie otherwise this message will keep appearing
                Response.Cookies.Delete(Constants.Security.BackOfficeExternalCookieName);
            }

            return response();
        }

        private async Task<bool> AutoLinkAndSignInExternalAccount(ExternalLoginInfo loginInfo, ExternalSignInAutoLinkOptions autoLinkOptions)
        {
            if (autoLinkOptions == null)
                return false;

            if (autoLinkOptions.AutoLinkExternalAccount == false)
                return true; // TODO: This seems weird to return true, but it was like that before so must be a reason?

            var email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email);

            //we are allowing auto-linking/creating of local accounts
            if (email.IsNullOrWhiteSpace())
            {
                ViewData.SetExternalSignInProviderErrors(
                    new BackOfficeExternalLoginProviderErrors(
                        loginInfo.LoginProvider,
                        new[] { $"The requested provider ({loginInfo.LoginProvider}) has not provided the email claim {ClaimTypes.Email}, the account cannot be linked." }));
            }
            else
            {
                //Now we need to perform the auto-link, so first we need to lookup/create a user with the email address
                var autoLinkUser = await _userManager.FindByEmailAsync(email);
                if (autoLinkUser != null)
                {
                    try
                    {
                        //call the callback if one is assigned
                        autoLinkOptions.OnAutoLinking?.Invoke(autoLinkUser, loginInfo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Could not link login provider {LoginProvider}.", loginInfo.LoginProvider);
                        ViewData.SetExternalSignInProviderErrors(
                            new BackOfficeExternalLoginProviderErrors(
                                loginInfo.LoginProvider,
                                new[] { "Could not link login provider " + loginInfo.LoginProvider + ". " + ex.Message }));
                        return true;
                    }

                    await LinkUser(autoLinkUser, loginInfo);
                }
                else
                {
                    var name = loginInfo.Principal?.Identity?.Name;
                    if (name.IsNullOrWhiteSpace()) throw new InvalidOperationException("The Name value cannot be null");

                    autoLinkUser = BackOfficeIdentityUser.CreateNew(_globalSettings, email, email, autoLinkOptions.GetUserAutoLinkCulture(_globalSettings), name);

                    foreach (var userGroup in autoLinkOptions.DefaultUserGroups)
                    {
                        autoLinkUser.AddRole(userGroup);
                    }

                    //call the callback if one is assigned
                    try
                    {
                        autoLinkOptions.OnAutoLinking?.Invoke(autoLinkUser, loginInfo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Could not link login provider {LoginProvider}.", loginInfo.LoginProvider);
                        ViewData.SetExternalSignInProviderErrors(
                            new BackOfficeExternalLoginProviderErrors(
                                loginInfo.LoginProvider,
                                new[] { "Could not link login provider " + loginInfo.LoginProvider + ". " + ex.Message }));
                        return true;
                    }

                    var userCreationResult = await _userManager.CreateAsync(autoLinkUser);

                    if (userCreationResult.Succeeded == false)
                    {
                        ViewData.SetExternalSignInProviderErrors(
                            new BackOfficeExternalLoginProviderErrors(
                                loginInfo.LoginProvider,
                                userCreationResult.Errors.Select(x => x.Description).ToList()));
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
            var existingLogins = await _userManager.GetLoginsAsync(autoLinkUser);
            var exists = existingLogins.FirstOrDefault(x => x.LoginProvider == loginInfo.LoginProvider && x.ProviderKey == loginInfo.ProviderKey);

            // if it already exists (perhaps it was added in the AutoLink callbak) then we just continue
            if (exists != null)
            {
                //sign in
                await _signInManager.SignInAsync(autoLinkUser, isPersistent: false);
                return;
            }

            var linkResult = await _userManager.AddLoginAsync(autoLinkUser, loginInfo);
            if (linkResult.Succeeded)
            {
                //we're good! sign in
                await _signInManager.SignInAsync(autoLinkUser, isPersistent: false);
                return;
            }

            ViewData.SetExternalSignInProviderErrors(
                   new BackOfficeExternalLoginProviderErrors(
                       loginInfo.LoginProvider,
                       linkResult.Errors.Select(x => x.Description).ToList()));

            //If this fails, we should really delete the user since it will be in an inconsistent state!
            var deleteResult = await _userManager.DeleteAsync(autoLinkUser);
            if (!deleteResult.Succeeded)
            {
                //DOH! ... this isn't good, combine all errors to be shown
                ViewData.SetExternalSignInProviderErrors(
                    new BackOfficeExternalLoginProviderErrors(
                        loginInfo.LoginProvider,
                        linkResult.Errors.Concat(deleteResult.Errors).Select(x => x.Description).ToList()));
            }
        }

        // Used for XSRF protection when adding external logins
        // TODO: This is duplicated in BackOfficeSignInManager
        private const string XsrfKey = "XsrfId";

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return Redirect("/");
        }


    }
}
