using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Strings;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using umbraco;

namespace Umbraco.Web.WebServices
{
    /// <summary>
    /// A REST controller used for accessing Core.Strings services.
    /// </summary>
    public class CoreStringsController : UmbracoAuthorizedController
    {
        [HttpGet]
        [ValidateInput(false)]
        public JsonResult ToSafeAlias(string value, bool camelCase = true)
        {
            // always return a proper camel-cased alias
            // when checking... javascript does a case-unsensitive comparison
            return value == null 
                ? Json(new {error = "no value."}, JsonRequestBehavior.AllowGet)
                : Json(new { alias = value.ToCleanString(CleanStringType.Alias | CleanStringType.CamelCase) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JavaScriptResult ServicesJavaScript()
        {
            var controllerPath = Url.GetCoreStringsControllerPath();
            var js = ShortStringHelperResolver.Current.Helper.GetShortStringServicesJavaScript(controllerPath);
            return JavaScript(js);
        }
    }
}