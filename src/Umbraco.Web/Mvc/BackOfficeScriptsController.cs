using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Umbraco.Core.Configuration;
using Umbraco.Core.Manifest;
using Umbraco.Core;
using Umbraco.Web.UI.JavaScript;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// A controller to return javascript content from the server
    /// </summary>
    public class BackOfficeScriptsController : Controller
    {

        /// <summary>
        /// Returns the RequireJS file including all references found in manifests
        /// </summary>
        /// <returns></returns>
        public JavaScriptResult Application()
        {
            var plugins = new DirectoryInfo(Server.MapPath("~/App_Plugins"));
            var parser = new ManifestParser(plugins);
            var requireJs = new RequireJsInit(parser);
            var result = requireJs.GetJavascriptInitialization(RequireJsInit.GetDefaultConfig(), RequireJsInit.GetDefaultInitialization());
            return JavaScript(result);
        }
        
        /// <summary>
        /// Returns the JavaScript object representing the static server variables javascript object
        /// </summary>
        /// <returns></returns>
        public JavaScriptResult ServerVariables()
        {            
            //now we need to build up the variables
            var d = new Dictionary<string, object>
                {
                    {"umbracoPath", GlobalSettings.Path},
                    {"contentEditorApiBaseUrl", Url.GetUmbracoApiService<ContentEditorApiController>("PostSaveContent").TrimEnd("PostSaveContent")}
                };

            return JavaScript(ServerVariablesParser.Parse(d));
        }

    }
}
