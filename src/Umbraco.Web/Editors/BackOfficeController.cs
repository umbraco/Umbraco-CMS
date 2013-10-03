using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
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
            var initJs = new JsInitialization(parser);
            var initCss = new CssInitialization(parser);


            var result = initJs.GetJavascriptInitialization(JsInitialization.GetDefaultInitialization());
            result += initCss.GetStylesheetInitialization();
           
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
                                {"macroApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<MacroController>("GetMacroParameters")},
                                {"authenticationApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<AuthenticationController>("PostLogin")},
                                {"userApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<UserController>("GetAll")},
                                {"legacyApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<LegacyController>("DeleteLegacyItem")},
                                {"entityApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<EntityController>("GetById")},
                                {"dataTypeApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<DataTypeController>("GetById")},
                                {"dashboardApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<DashboardController>("GetDashboard")},
                                {"logApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<LogController>("GetEntityLog")},
                                {"memberApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<MemberController>("GetByLogin")},
                            }
                    },
                    {
                        "umbracoSettings", new Dictionary<string, object>
                            {
                                {"umbracoPath", GlobalSettings.Path},
                                {"appPluginsPath", IOHelper.ResolveUrl(SystemDirectories.AppPlugins).TrimEnd('/')},
                                {"imageFileTypes", 
                                    string.Join(",",UmbracoConfig.For.UmbracoSettings().Content.ImageFileTypes)},
                            }
                    },
                    {
                        "umbracoPlugins", new Dictionary<string, object>
                            {
                                {"trees", GetTreePluginsMetaData()}                                
                            }
                    },  
                    { "isDebuggingEnabled", HttpContext.IsDebuggingEnabled }
                };

            return JavaScript(ServerVariablesParser.Parse(d));
        }

        private IEnumerable<Dictionary<string, string>> GetTreePluginsMetaData()
        {
            var treeTypes = PluginManager.Current.ResolveAttributedTreeControllers();
            //get all plugin trees with their attributes
            var treesWithAttributes = treeTypes.Select(x => new
                {
                    tree = x, attributes =                     
                    x.GetCustomAttributes(false)
                }).ToArray();
            
            var pluginTreesWithAttributes = treesWithAttributes
                //don't resolve any tree decorated with CoreTreeAttribute
                .Where(x => x.attributes.All(a => (a is CoreTreeAttribute) == false))
                //we only care about trees with the PluginControllerAttribute
                .Where(x => x.attributes.Any(a => a is PluginControllerAttribute))
                .ToArray();

            return (from p in pluginTreesWithAttributes
                    let treeAttr = p.attributes.OfType<TreeAttribute>().Single()
                    let pluginAttr = p.attributes.OfType<PluginControllerAttribute>().Single()
                    select new Dictionary<string, string>
                        {
                            {"alias", treeAttr.Alias}, {"packageFolder", pluginAttr.AreaName}
                        }).ToArray();

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
