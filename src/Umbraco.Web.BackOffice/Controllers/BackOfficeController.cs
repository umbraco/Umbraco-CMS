using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
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
using Umbraco.Web.BackOffice.ActionResults;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.BackOffice.Security;
using Umbraco.Web.Common.ActionsResults;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Authorization;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Common.Security;
using Umbraco.Web.Models;
using Umbraco.Web.WebAssets;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Controllers
{
    [DisableBrowserCache]
    [UmbracoRequireHttps]
    [PluginController(Constants.Web.Mvc.BackOfficeArea)]
    [IsBackOffice]
    public class BackOfficeController : UmbracoController
    {
        // NOTE: Each action must either be explicitly authorized or explicitly [AllowAnonymous], the latter is optional because
        // this controller itself doesn't require authz but it's more clear what the intention is.

        private readonly IBackOfficeUserManager _userManager;
        private readonly IRuntimeMinifier _runtimeMinifier;
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILocalizedTextService _textService;
        private readonly IGridConfig _gridConfig;
        private readonly BackOfficeServerVariables _backOfficeServerVariables;
        private readonly AppCaches _appCaches;
        private readonly IBackOfficeSignInManager _signInManager;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly ILogger<BackOfficeController> _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IBackOfficeExternalLoginProviders _externalLogins;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBackOfficeTwoFactorOptions _backOfficeTwoFactorOptions;
        private readonly ServerVariablesParser _serverVariables;

        public BackOfficeController(
            IBackOfficeUserManager userManager,
            IRuntimeMinifier runtimeMinifier,
            IOptions<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment,
            ILocalizedTextService textService,
            IGridConfig gridConfig,
            BackOfficeServerVariables backOfficeServerVariables,
            AppCaches appCaches,
            IBackOfficeSignInManager signInManager,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            ILogger<BackOfficeController> logger,
            IJsonSerializer jsonSerializer,
            IBackOfficeExternalLoginProviders externalLogins,
            IHttpContextAccessor httpContextAccessor,
            IBackOfficeTwoFactorOptions backOfficeTwoFactorOptions,
            ServerVariablesParser serverVariables)
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
            _httpContextAccessor = httpContextAccessor;
            _backOfficeTwoFactorOptions = backOfficeTwoFactorOptions;
            _serverVariables = serverVariables;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Default()
        {
            // force authentication to occur since this is not an authorized endpoint
            var result = await this.AuthenticateBackOfficeAsync();

            var viewPath = Path.Combine(_globalSettings.UmbracoPath , Constants.Web.Mvc.BackOfficeArea, nameof(Default) + ".cshtml")
                .Replace("\\", "/"); // convert to forward slashes since it's a virtual path

            return await RenderDefaultOrProcessExternalLoginAsync(
                result,
                () => View(viewPath),
                () => View(viewPath));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyInvite(string invite)
        {
            var authenticate = await this.AuthenticateBackOfficeAsync();

            //if you are hitting VerifyInvite, you're already signed in as a different user, and the token is invalid
            //you'll exit on one of the return RedirectToAction(nameof(Default)) but you're still logged in so you just get
            //dumped at the default admin view with no detail
            if (authenticate.Succeeded)
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
        [AllowAnonymous]
        public async Task<IActionResult> AuthorizeUpgrade()
        {
            // force authentication to occur since this is not an authorized endpoint
            var result = await this.AuthenticateBackOfficeAsync();

            var viewPath = Path.Combine(_globalSettings.UmbracoPath, Constants.Web.Mvc.BackOfficeArea, nameof(AuthorizeUpgrade) + ".cshtml");

            return await RenderDefaultOrProcessExternalLoginAsync(
                result,
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
        [AllowAnonymous]
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
        [AllowAnonymous]
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

        [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
        [HttpGet]
        public IEnumerable<IGridEditorConfig> GetGridConfig()
        {
            return _gridConfig.EditorsConfig.Editors;
        }

        /// <summary>
        /// Returns the JavaScript object representing the static server variables javascript object
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
        [MinifyJavaScriptResult(Order = 1)]
        public async Task<JavaScriptResult> ServerVariables()
        {
            // cache the result if debugging is disabled
            var serverVars = await _serverVariables.ParseAsync(await _backOfficeServerVariables.GetServerVariablesAsync());
            var result = _hostingEnvironment.IsDebugMode
                ? serverVars
                : _appCaches.RuntimeCache.GetCacheItem<string>(
                    typeof(BackOfficeController) + "ServerVariables",
                    () => serverVars,
                    new TimeSpan(0, 10, 0));

            return new JavaScriptResult(result);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ExternalLogin(string provider, string redirectUrl = null)
        {
            if (redirectUrl == null)
            {
                redirectUrl = Url.Action(nameof(Default), this.GetControllerName());
            }

            // Configures the redirect URL and user identifier for the specified external login
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            
            return Challenge(properties, provider);
        }

        /// <summary>
        /// Called when a user links an external login provider in the back office
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
        [HttpPost]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action(nameof(ExternalLinkLoginCallback), this.GetControllerName());

            // Configures the redirect URL and user identifier for the specified external login including xsrf data
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
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
        [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
        [HttpGet]
        public async Task<IActionResult> ExternalLinkLoginCallback()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // ... this should really not happen
                TempData[ViewDataExtensions.TokenExternalSignInError] = new[] { "Local user does not exist" };
                return RedirectToLocal(Url.Action(nameof(Default), this.GetControllerName()));
            }

            var loginInfo = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));

            if (loginInfo == null)
            {
                //Add error and redirect for it to be displayed
                TempData[ViewDataExtensions.TokenExternalSignInError] = new[] { "An error occurred, could not get external login info" };
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
            AuthenticateResult authenticateResult,
            Func<IActionResult> defaultResponse,
            Func<IActionResult> externalSignInResponse)
        {
            if (defaultResponse is null) throw new ArgumentNullException(nameof(defaultResponse));
            if (externalSignInResponse is null) throw new ArgumentNullException(nameof(externalSignInResponse));

            ViewData.SetUmbracoPath(_globalSettings.GetUmbracoMvcArea(_hostingEnvironment));

            //check if there is the TempData or cookies with the any token name specified, if so, assign to view bag and render the view
            if (ViewData.FromBase64CookieData<BackOfficeExternalLoginProviderErrors>(_httpContextAccessor.HttpContext, ViewDataExtensions.TokenExternalSignInError, _jsonSerializer) ||
                ViewData.FromTempData(TempData, ViewDataExtensions.TokenExternalSignInError) ||
                ViewData.FromTempData(TempData, ViewDataExtensions.TokenPasswordResetCode))
                return defaultResponse();

            //First check if there's external login info, if there's not proceed as normal
            var loginInfo = await _signInManager.GetExternalLoginInfoAsync();

            if (loginInfo == null || loginInfo.Principal == null)
            {
                // if the user is not logged in, check if there's any auto login redirects specified
                if (!authenticateResult.Succeeded)
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

            // Sign in the user with this external login provider (which auto links, etc...)
            var result = await _signInManager.ExternalLoginSignInAsync(loginInfo, isPersistent: false);

            var errors = new List<string>();

            if (result == Microsoft.AspNetCore.Identity.SignInResult.Success)
            {

            }
            else if (result == Microsoft.AspNetCore.Identity.SignInResult.TwoFactorRequired)
            {

                var attemptedUser = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
                if (attemptedUser == null)
                {
                    return new ValidationErrorResult($"No local user found for the login provider {loginInfo.LoginProvider} - {loginInfo.ProviderKey}");
                }

                var twofactorView = _backOfficeTwoFactorOptions.GetTwoFactorView(attemptedUser.UserName);
                if (twofactorView.IsNullOrWhiteSpace())
                {
                    return new ValidationErrorResult($"The registered {typeof(IBackOfficeTwoFactorOptions)} of type {_backOfficeTwoFactorOptions.GetType()} did not return a view for two factor auth ");
                }

                // create a with information to display a custom two factor send code view
                var verifyResponse = new ObjectResult(new
                {
                    twoFactorView = twofactorView,
                    userId = attemptedUser.Id
                })
                {
                    StatusCode = StatusCodes.Status402PaymentRequired
                };

                return verifyResponse;

            }
            else if (result == Microsoft.AspNetCore.Identity.SignInResult.LockedOut)
            {
                errors.Add($"The local user {loginInfo.Principal.Identity.Name} for the external provider {loginInfo.ProviderDisplayName} is locked out.");
            }
            else if (result == Microsoft.AspNetCore.Identity.SignInResult.NotAllowed)
            {
                // This occurs when SignInManager.CanSignInAsync fails which is when RequireConfirmedEmail , RequireConfirmedPhoneNumber or RequireConfirmedAccount fails
                // however since we don't enforce those rules (yet) this shouldn't happen.
                errors.Add($"The user {loginInfo.Principal.Identity.Name} for the external provider {loginInfo.ProviderDisplayName} has not confirmed their details and cannot sign in.");
            }
            else if (result == Microsoft.AspNetCore.Identity.SignInResult.Failed)
            {
                // Failed only occurs when the user does not exist
                errors.Add("The requested provider (" + loginInfo.LoginProvider + ") has not been linked to an account, the provider must be linked from the back office.");
            }
            else if (result == AutoLinkSignInResult.FailedNotLinked)
            {
                errors.Add("The requested provider (" + loginInfo.LoginProvider + ") has not been linked to an account, the provider must be linked from the back office.");
            }
            else if (result == AutoLinkSignInResult.FailedNoEmail)
            {
                errors.Add($"The requested provider ({loginInfo.LoginProvider}) has not provided the email claim {ClaimTypes.Email}, the account cannot be linked.");                
            }
            else if (result is AutoLinkSignInResult autoLinkSignInResult && autoLinkSignInResult.Errors.Count > 0)
            {
                errors.AddRange(autoLinkSignInResult.Errors);
            }

            if (errors.Count > 0)
            {
                ViewData.SetExternalSignInProviderErrors(
                    new BackOfficeExternalLoginProviderErrors(
                        loginInfo.LoginProvider,
                        errors));
            }

            return response();
        }

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
