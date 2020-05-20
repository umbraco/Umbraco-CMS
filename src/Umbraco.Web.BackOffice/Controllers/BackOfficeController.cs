using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.Hosting;
using Umbraco.Core.Services;
using Umbraco.Core.WebAssets;
using Umbraco.Extensions;
using Umbraco.Net;
using Umbraco.Web.BackOffice.ActionResults;
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
        private readonly IUmbracoApplicationLifetime _umbracoApplicationLifetime;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILocalizedTextService _textService;
        private readonly IGridConfig _gridConfig;
        private readonly BackOfficeServerVariables _backOfficeServerVariables;
        private readonly AppCaches _appCaches;

        public BackOfficeController(
            BackOfficeUserManager userManager,
            IRuntimeMinifier runtimeMinifier,
            IGlobalSettings globalSettings,
            IHostingEnvironment hostingEnvironment,
            IUmbracoApplicationLifetime umbracoApplicationLifetime,
            IUmbracoContextAccessor umbracoContextAccessor,
            ILocalizedTextService textService,
            IGridConfig gridConfig,
            BackOfficeServerVariables backOfficeServerVariables,
            AppCaches appCaches)
        {
            _userManager = userManager;
            _runtimeMinifier = runtimeMinifier;
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
            _umbracoApplicationLifetime = umbracoApplicationLifetime;
            _umbracoContextAccessor = umbracoContextAccessor;
            _textService = textService;
            _gridConfig = gridConfig ?? throw new ArgumentNullException(nameof(gridConfig));
            _backOfficeServerVariables = backOfficeServerVariables;
            _appCaches = appCaches;
        }

        [HttpGet]
        public IActionResult Default()
        {
            // TODO: Migrate this
            return View();
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
        public JsonNetResult LocalizedText(string culture = null)
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

            return new JsonNetResult { Data = nestedDictionary, Formatting = Formatting.None };
        }

        [UmbracoAuthorize(Order = 0)] // TODO: Re-implement UmbracoAuthorizeAttribute
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
        public async Task<ActionResult> ValidatePasswordResetCode([Bind(Prefix = "u")]int userId, [Bind(Prefix = "r")]string resetCode)
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

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return Redirect("/");
        }
    }
}
