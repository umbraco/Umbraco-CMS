using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Mvc;
using Umbraco.Core.Configuration;
using Umbraco.Core.Manifest;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Umbraco.Web.UI.JavaScript;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// A controller to render out the default back office view and JS results
    /// </summary>
    public class BackOfficeController : UmbracoController
    {

        /// <summary>
        /// Render the default view
        /// </summary>
        /// <returns></returns>
        public ActionResult Default()
        {
            return View(GlobalSettings.Path.EnsureEndsWith('/') + "Views/Default.cshtml");
        }

        /// <summary>
        /// Returns the RequireJS file including all references found in manifests
        /// </summary>
        /// <returns></returns>
        public JavaScriptResult Application()
        {
            var plugins = new DirectoryInfo(Server.MapPath("~/App_Plugins"));
            var parser = new ManifestParser(plugins);
            var requireJs = new JsInitialization(parser);
            var result = requireJs.GetJavascriptInitialization(JsInitialization.GetDefaultInitialization());
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
                    {
                        "umbracoUrls", new Dictionary<string, object>
                            {
                                {"legacyTreeJs", Url.Action("LegacyTreeJs", "BackOffice")},                    
                                //API URLs
                                {"contentApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<ContentController>("PostSave")},
                                {"mediaApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<MediaController>("GetRootMedia")},
                                {"sectionApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<SectionController>("GetSections")},
                                {"treeApplicationApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<ApplicationTreeController>("GetApplicationTrees")},
                                {"contentTypeApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<ContentTypeController>("GetAllowedChildren")},
                                {"mediaTypeApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<MediaTypeController>("GetAllowedChildren")},
                                {"authenticationApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<AuthenticationController>("PostLogin")},
                                {"legacyApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<LegacyController>("DeleteLegacyItem")}
                            }
                    },
                    {
                        "umbracoSettings", new Dictionary<string, object>
                            {
                                {"umbracoPath", GlobalSettings.Path},
                                {"imageFileTypes", UmbracoSettings.ImageFileTypes},
                            }
                    },
                    { "isDebuggingEnabled", HttpContext.IsDebuggingEnabled }
                };

            return JavaScript(ServerVariablesParser.Parse(d));
        }

        /// <summary>
        /// Returns the JavaScript blocks for any legacy trees declared
        /// </summary>
        /// <returns></returns>
        [UmbracoAuthorize]
        public JavaScriptResult LegacyTreeJs()
        {            
            var javascript = new StringBuilder();
            javascript.AppendLine(LegacyTreeJavascript.GetLegacyTreeJavascript());
            javascript.AppendLine(LegacyTreeJavascript.GetLegacyIActionJavascript());
            return JavaScript(javascript.ToString());
        }

        

    }
}
