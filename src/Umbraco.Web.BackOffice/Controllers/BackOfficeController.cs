using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.Hosting;
using Umbraco.Core.Services;
using Umbraco.Core.WebAssets;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.ActionResults;
using Umbraco.Web.Models;
using Umbraco.Web.WebAssets;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Controllers
{

    [Area(Constants.Web.Mvc.BackOfficeArea)]
    public class BackOfficeController : Controller
    {
        private readonly BackOfficeUserManager _userManager;
        private readonly IRuntimeMinifier _runtimeMinifier;
        private readonly IGlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILocalizedTextService _textService;
        private readonly IGridConfig _gridConfig;
        private readonly BackOfficeServerVariables _backOfficeServerVariables;
        private readonly AppCaches _appCaches;
        private readonly SignInManager<BackOfficeIdentityUser> _signInManager;

        public BackOfficeController(
            BackOfficeUserManager userManager,
            IRuntimeMinifier runtimeMinifier,
            IGlobalSettings globalSettings,
            IHostingEnvironment hostingEnvironment,

            IUmbracoContextAccessor umbracoContextAccessor,
            ILocalizedTextService textService,
            IGridConfig gridConfig,
            BackOfficeServerVariables backOfficeServerVariables,
            AppCaches appCaches,
            SignInManager<BackOfficeIdentityUser> signInManager // TODO: Review this, do we want it/need it or create our own?
            )
        {
            _userManager = userManager;
            _runtimeMinifier = runtimeMinifier;
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
            _umbracoContextAccessor = umbracoContextAccessor;
            _textService = textService;
            _gridConfig = gridConfig ?? throw new ArgumentNullException(nameof(gridConfig));
            _backOfficeServerVariables = backOfficeServerVariables;
            _appCaches = appCaches;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Default()
        {
            return await RenderDefaultOrProcessExternalLoginAsync(
                () => View(),
                () => View());
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
            var securityHelper = _umbracoContextAccessor.GetRequiredUmbracoContext().Security;
            var isAuthenticated = securityHelper.IsAuthenticated();

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
                var result = await _userManager.VerifyUserTokenAsync(user, "ResetPassword", "ResetPassword", resetCode);
                if (result)
                {
                    //Add a flag and redirect for it to be displayed
                    TempData[ViewDataExtensions.TokenPasswordResetCode] = new ValidatePasswordResetCodeModel { UserId = userId, ResetCode = resetCode };
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
        private async Task<IActionResult> RenderDefaultOrProcessExternalLoginAsync(
            Func<IActionResult> defaultResponse,
            Func<IActionResult> externalSignInResponse)
        {
            if (defaultResponse is null) throw new ArgumentNullException(nameof(defaultResponse));
            if (externalSignInResponse is null) throw new ArgumentNullException(nameof(externalSignInResponse));

            ViewData.SetUmbracoPath(_globalSettings.GetUmbracoMvcArea(_hostingEnvironment));

            //check if there is the TempData with the any token name specified, if so, assign to view bag and render the view
            if (ViewData.FromTempData(TempData, ViewDataExtensions.TokenExternalSignInError) ||
                ViewData.FromTempData(TempData, ViewDataExtensions.TokenPasswordResetCode))
                return defaultResponse();

            return defaultResponse();

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
