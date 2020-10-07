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
using Umbraco.Web.Security;
using Umbraco.Web.WebAssets;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Controllers
{
    //[UmbracoRequireHttps] //TODO Reintroduce
    [DisableBrowserCache]
    [PluginController(Constants.Web.Mvc.BackOfficeArea)]
    public class BackOfficeController : Controller
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
        private readonly IBackofficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly ILogger<BackOfficeController> _logger;
        private readonly IJsonSerializer _jsonSerializer;

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
            IBackofficeSecurityAccessor backofficeSecurityAccessor,
            ILogger<BackOfficeController> logger,
            IJsonSerializer jsonSerializer)
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
            if (_backofficeSecurityAccessor.BackofficeSecurity.IsAuthenticated())
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
            var isAuthenticated = _backofficeSecurityAccessor.BackofficeSecurity.IsAuthenticated();

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
                    return RedirectToLocal(Url.Action("Default", "BackOffice"));
                }
            }

            //Add error and redirect for it to be displayed
            TempData[ViewDataExtensions.TokenPasswordResetCode] = new[] { _textService.Localize("login/resetCodeExpired") };
            return RedirectToLocal(Url.Action("Default", "BackOffice"));
        }

        /// <summary>
        /// Used by Default and AuthorizeUpgrade to render as per normal if there's no external login info,
        /// otherwise process the external login info.
        /// </summary>
        /// <returns></returns>
        private Task<IActionResult> RenderDefaultOrProcessExternalLoginAsync(
            Func<IActionResult> defaultResponse,
            Func<IActionResult> externalSignInResponse)
        {
            if (defaultResponse is null) throw new ArgumentNullException(nameof(defaultResponse));
            if (externalSignInResponse is null) throw new ArgumentNullException(nameof(externalSignInResponse));

            ViewData.SetUmbracoPath(_globalSettings.GetUmbracoMvcArea(_hostingEnvironment));

            //check if there is the TempData with the any token name specified, if so, assign to view bag and render the view
            if (ViewData.FromTempData(TempData, ViewDataExtensions.TokenExternalSignInError) ||
                ViewData.FromTempData(TempData, ViewDataExtensions.TokenPasswordResetCode))
                return Task.FromResult(defaultResponse());

            return Task.FromResult(defaultResponse());

            //First check if there's external login info, if there's not proceed as normal
            // TODO: Review this, not sure if this will work as expected until we integrate OAuth
            // TODO: Do we pass in XsrfKey ? need to investigate how this all works now
            //var loginInfo = await _signInManager.GetExternalLoginInfoAsync();

            //if (loginInfo == null || loginInfo.ExternalIdentity.IsAuthenticated == false)
            //{
            //    return defaultResponse();
            //}

            ////we're just logging in with an external source, not linking accounts
            //return await ExternalSignInAsync(loginInfo, externalSignInResponse);
        }

        // Used for XSRF protection when adding external logins
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
