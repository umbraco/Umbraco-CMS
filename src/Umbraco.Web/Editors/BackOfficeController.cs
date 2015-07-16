using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.UI;
using dotless.Core.Parser.Tree;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Manifest;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Security;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Umbraco.Web.UI.JavaScript;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.Models;
using Umbraco.Web.WebServices;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// A controller to render out the default back office view and JS results
    /// </summary>
    [UmbracoUseHttps]
    [DisableClientCache]
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
        /// This Action is used by the installer when an upgrade is detected but the admin user is not logged in. We need to 
        /// ensure the user is authenticated before the install takes place so we redirect here to show the standard login screen.
        /// </summary>
        /// <returns></returns>      
        [HttpGet]
        public ActionResult AuthorizeUpgrade()
        {
            return View(GlobalSettings.Path.EnsureEndsWith('/') + "Views/AuthorizeUpgrade.cshtml");
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
                ? User.Identity.IsAuthenticated && User.Identity is UmbracoBackOfficeIdentity
                    ? Security.CurrentUser.GetUserCulture(Services.TextService) 
                    : CultureInfo.GetCultureInfo("en")
                : CultureInfo.GetCultureInfo(culture);

            var textForCulture = Services.TextService.GetAllStoredValues(cultureInfo)
                //the dictionary returned is fine but the delimiter between an 'area' and a 'value' is a '/' but the javascript
                // in the back office requres the delimiter to be a '_' so we'll just replace it
                .ToDictionary(key => key.Key.Replace("/", "_"), val => val.Value);

            return new JsonNetResult { Data = textForCulture, Formatting = Formatting.Indented };
        }

        /// <summary>
        /// Returns the JavaScript main file including all references found in manifests
        /// </summary>
        /// <returns></returns>
        [MinifyJavaScriptResult(Order = 0)]
        [OutputCache(Order = 1, VaryByParam = "none", Location = OutputCacheLocation.Server, Duration = 5000)]
        public JavaScriptResult Application()
        {
            var plugins = new DirectoryInfo(Server.MapPath("~/App_Plugins"));
            var parser = new ManifestParser(plugins, ApplicationContext.ApplicationCache.RuntimeCache);
            var initJs = new JsInitialization(parser);
            var initCss = new CssInitialization(parser);

            //get the legacy ActionJs file references to append as well
            var legacyActionJsRef = new JArray(GetLegacyActionJs(LegacyJsActionType.JsUrl));
            
            var result = initJs.GetJavascriptInitialization(HttpContext, JsInitialization.GetDefaultInitialization(), legacyActionJsRef);
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
            Func<JArray> getResult = () =>
            {
                var plugins = new DirectoryInfo(Server.MapPath("~/App_Plugins"));
                var parser = new ManifestParser(plugins, ApplicationContext.ApplicationCache.RuntimeCache);
                var initJs = new JsInitialization(parser);
                var initCss = new CssInitialization(parser);
                var jsResult = initJs.GetJavascriptInitializationArray(HttpContext, new JArray());
                var cssResult = initCss.GetStylesheetInitializationArray(HttpContext);
                ManifestParser.MergeJArrays(jsResult, cssResult);
                return jsResult;
            };

            //cache the result if debugging is disabled
            var result = HttpContext.IsDebuggingEnabled
                ? getResult()
                : ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<JArray>(
                    typeof (BackOfficeController) + "GetManifestAssetList",
                    () => getResult(),
                    new TimeSpan(0, 10, 0));

            return new JsonNetResult { Data = result, Formatting = Formatting.Indented };
        }
        
        [UmbracoAuthorize(Order = 0)]
        [HttpGet]
        public JsonNetResult GetGridConfig()
        {
            Func<List<GridEditor>> getResult = () =>
            {
                var editors = new List<GridEditor>();
                var gridConfig = Server.MapPath("~/Config/grid.editors.config.js");
                if (System.IO.File.Exists(gridConfig))
                {
                    try
                    {
                        var arr = JArray.Parse(System.IO.File.ReadAllText(gridConfig));
                        //ensure the contents parse correctly to objects
                        var parsed = ManifestParser.GetGridEditors(arr);
                        editors.AddRange(parsed);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<BackOfficeController>("Could not parse the contents of grid.editors.config.js into a JSON array", ex);
                    }
                }

                var plugins = new DirectoryInfo(Server.MapPath("~/App_Plugins"));
                var parser = new ManifestParser(plugins, ApplicationContext.ApplicationCache.RuntimeCache);
                var builder = new ManifestBuilder(ApplicationContext.ApplicationCache.RuntimeCache, parser);
                foreach (var gridEditor in builder.GridEditors)
                {
                    //no duplicates! (based on alias)
                    if (editors.Contains(gridEditor) == false)
                    {
                        editors.Add(gridEditor);
                    }
                }
                return editors;
            };

            //cache the result if debugging is disabled
            var result = HttpContext.IsDebuggingEnabled
                ? getResult()
                : ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<List<GridEditor>>(
                    typeof(BackOfficeController) + "GetGridConfig",
                    () => getResult(),
                    new TimeSpan(0, 10, 0));
            
            return new JsonNetResult { Data = result, Formatting = Formatting.Indented };
        }

        /// <summary>
        /// Returns the JavaScript object representing the static server variables javascript object
        /// </summary>
        /// <returns></returns>
        [UmbracoAuthorize(Order = 0)]
        [MinifyJavaScriptResult(Order = 1)] 
        public JavaScriptResult ServerVariables()
        {
            Func<string> getResult = () =>
            {                
                var defaultVals = new Dictionary<string, object>
                {
                    {
                        "umbracoUrls", new Dictionary<string, object>
                        {
                            {"legacyTreeJs", Url.Action("LegacyTreeJs", "BackOffice")},
                            {"manifestAssetList", Url.Action("GetManifestAssetList", "BackOffice")},
                            {"gridConfig", Url.Action("GetGridConfig", "BackOffice")},
                            {"serverVarsJs", Url.Action("Application", "BackOffice")},
                            //API URLs
                            {
                                "embedApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<RteEmbedController>(
                                    controller => controller.GetEmbed("", 0, 0))
                            },
                            {
                                "userApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<UserController>(
                                    controller => controller.PostDisableUser(0))
                            },
                            {
                                "contentApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<ContentController>(
                                    controller => controller.PostSave(null))
                            },
                            {
                                "mediaApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<MediaController>(
                                    controller => controller.GetRootMedia())
                            },
                            {
                                "imagesApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<ImagesController>(
                                    controller => controller.GetBigThumbnail(0))
                            },
                            {
                                "sectionApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<SectionController>(
                                    controller => controller.GetSections())
                            },
                            {
                                "treeApplicationApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<ApplicationTreeController>(
                                    controller => controller.GetApplicationTrees(null, null, null))
                            },
                            {
                                "contentTypeApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<ContentTypeController>(
                                    controller => controller.GetAllowedChildren(0))
                            },
                            {
                                "mediaTypeApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<MediaTypeController>(
                                    controller => controller.GetAllowedChildren(0))
                            },
                            {
                                "macroApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<MacroController>(
                                    controller => controller.GetMacroParameters(0))
                            },
                            {
                                "authenticationApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<AuthenticationController>(
                                    controller => controller.PostLogin(null))
                            },
                            {
                                "currentUserApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<CurrentUserController>(
                                    controller => controller.GetMembershipProviderConfig())
                            },
                            {
                                "legacyApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<LegacyController>(
                                    controller => controller.DeleteLegacyItem(null, null, null))
                            },
                            {
                                "entityApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<EntityController>(
                                    controller => controller.GetById(0, UmbracoEntityTypes.Media))
                            },
                            {
                                "dataTypeApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<DataTypeController>(
                                    controller => controller.GetById(0))
                            },
                            {
                                "dashboardApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<DashboardController>(
                                    controller => controller.GetDashboard(null))
                            },
                            {
                                "logApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<LogController>(
                                    controller => controller.GetEntityLog(0))
                            },
                            {
                                "memberApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<MemberController>(
                                    controller => controller.GetByKey(Guid.Empty))
                            },
                            {
                                "packageInstallApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<PackageInstallController>(
                                    controller => controller.Fetch(string.Empty))
                            },
                            {
                                "rteApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<RichTextPreValueController>(
                                    controller => controller.GetConfiguration())
                            },
                            {
                                "stylesheetApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<StylesheetController>(
                                    controller => controller.GetAll())
                            },
                            {
                                "memberTypeApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<MemberTypeController>(
                                    controller => controller.GetAllTypes())
                            },
                            {
                                "updateCheckApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<UpdateCheckController>(
                                    controller => controller.GetCheck())
                            },
                            {
                                "tagApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<TagsController>(
                                    controller => controller.GetAllTags(null))
                            },
                            {
                                "memberTreeBaseUrl", Url.GetUmbracoApiServiceBaseUrl<MemberTreeController>(
                                    controller => controller.GetNodes("-1", null))
                            },
                            {
                                "mediaTreeBaseUrl", Url.GetUmbracoApiServiceBaseUrl<MediaTreeController>(
                                    controller => controller.GetNodes("-1", null))
                            },
                            {
                                "contentTreeBaseUrl", Url.GetUmbracoApiServiceBaseUrl<ContentTreeController>(
                                    controller => controller.GetNodes("-1", null))
                            },
                            {
                                "tagsDataBaseUrl", Url.GetUmbracoApiServiceBaseUrl<TagsDataController>(
                                    controller => controller.GetTags(""))
                            },
                            {
                                "examineMgmtBaseUrl", Url.GetUmbracoApiServiceBaseUrl<ExamineManagementApiController>(
                                    controller => controller.GetIndexerDetails())
                            },
                            {
                                "xmlDataIntegrityBaseUrl", Url.GetUmbracoApiServiceBaseUrl<XmlDataIntegrityController>(
                                    controller => controller.CheckContentXmlTable())
                            }
                        }
                    },
                    {
                        "umbracoSettings", new Dictionary<string, object>
                        {
                            {"umbracoPath", GlobalSettings.Path},
                            {"mediaPath", IOHelper.ResolveUrl(SystemDirectories.Media).TrimEnd('/')},
                            {"appPluginsPath", IOHelper.ResolveUrl(SystemDirectories.AppPlugins).TrimEnd('/')},
                            {
                                "imageFileTypes",
                                string.Join(",", UmbracoConfig.For.UmbracoSettings().Content.ImageFileTypes)
                            },
                            {"keepUserLoggedIn", UmbracoConfig.For.UmbracoSettings().Security.KeepUserLoggedIn},
                        }
                    },
                    {
                        "umbracoPlugins", new Dictionary<string, object>
                        {
                            {"trees", GetTreePluginsMetaData()}
                        }
                    },
                    {"isDebuggingEnabled", HttpContext.IsDebuggingEnabled},
                    {"application", GetApplicationState()}
                };

                //Parse the variables to a string
                return ServerVariablesParser.Parse(defaultVals);
            };

            //cache the result if debugging is disabled
            var result = HttpContext.IsDebuggingEnabled
                ? getResult()
                : ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<string>(
                    typeof(BackOfficeController) + "ServerVariables",
                    () => getResult(),
                    new TimeSpan(0, 10, 0));

            return JavaScript(result);
        }
        
        /// <summary>
        /// Returns the server variables regarding the application state
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetApplicationState()
        {
            if (ApplicationContext.IsConfigured == false)
                return null;

            var app = new Dictionary<string, object>
                {
                    {"assemblyVersion", UmbracoVersion.AssemblyVersion}
                };

            var version = string.IsNullOrEmpty(UmbracoVersion.CurrentComment)
                            ? UmbracoVersion.Current.ToString(3)
                            : string.Format("{0}-{1}", UmbracoVersion.Current.ToString(3), UmbracoVersion.CurrentComment);

            app.Add("version", version);
            app.Add("cdf", ClientDependency.Core.Config.ClientDependencySettings.Instance.Version);
            //useful for dealing with virtual paths on the client side when hosted in virtual directories especially
            app.Add("applicationPath", HttpContext.Request.ApplicationPath.EnsureEndsWith('/'));
            return app;
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
        [UmbracoAuthorize(Order = 0)]
        [MinifyJavaScriptResult(Order = 1)]
        public JavaScriptResult LegacyTreeJs()
        {
            Func<string> getResult = () =>
            {
                var javascript = new StringBuilder();
                javascript.AppendLine(LegacyTreeJavascript.GetLegacyTreeJavascript());
                javascript.AppendLine(LegacyTreeJavascript.GetLegacyIActionJavascript());
                //add all of the menu blocks
                foreach (var file in GetLegacyActionJs(LegacyJsActionType.JsBlock))
                {
                    javascript.AppendLine(file);
                }
                return javascript.ToString();
            };

            //cache the result if debugging is disabled
            var result = HttpContext.IsDebuggingEnabled
                ? getResult()
                : ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<string>(
                    typeof(BackOfficeController) + "LegacyTreeJs",
                    () => getResult(),
                    new TimeSpan(0, 10, 0));

            return JavaScript(result);
        }

        /// <summary>
        /// Renders out all JavaScript references that have bee declared in IActions
        /// </summary>
        private static IEnumerable<string> GetLegacyActionJs(LegacyJsActionType type)
        {
            var blockList = new List<string>();
            var urlList = new List<string>();
            foreach (var jsFile in global::umbraco.BusinessLogic.Actions.Action.GetJavaScriptFileReferences())
            {
                //validate that this is a url, if it is not, we'll assume that it is a text block and render it as a text
                //block instead.
                var isValid = true;
                
                if (Uri.IsWellFormedUriString(jsFile, UriKind.RelativeOrAbsolute))
                {
                    //ok it validates, but so does alert('hello'); ! so we need to do more checks

                    //here are the valid chars in a url without escaping
                    if (Regex.IsMatch(jsFile, @"[^a-zA-Z0-9-._~:/?#\[\]@!$&'\(\)*\+,%;=]"))
                        isValid = false;

                    //we'll have to be smarter and just check for certain js patterns now too!
                    var jsPatterns = new string[] {@"\+\s*\=", @"\);", @"function\s*\(", @"!=", @"=="};
                    if (jsPatterns.Any(p => Regex.IsMatch(jsFile, p)))
                    {
                        isValid = false;
                    }
                    if (isValid)
                    {
                        //it is a valid URL add to Url list
                        urlList.Add(IOHelper.ResolveUrl(jsFile));
                    }
                }
                else
                {
                    isValid = false;
                }

                if (isValid == false)
                {
                    //it isn't a valid URL, must be a js block
                    blockList.Add(jsFile);                     
                }
            }

            switch (type)
            {
                case LegacyJsActionType.JsBlock:
                    return blockList;
                case LegacyJsActionType.JsUrl:
                    return urlList;
            }

            return blockList;
        }
        
        private enum LegacyJsActionType
        {
            JsBlock,
            JsUrl
        }

    }
}
