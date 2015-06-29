﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using dotless.Core.Parser.Tree;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
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
using System.Web;
using AutoMapper;
using Microsoft.AspNet.Identity.Owin;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Task = System.Threading.Tasks.Task;
using Umbraco.Web.Security.Identity;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// A controller to render out the default back office view and JS results
    /// </summary>
    [UmbracoUseHttps]
    [DisableClientCache]
    public class BackOfficeController : UmbracoController
    {
        private BackOfficeUserManager _userManager;

        protected BackOfficeUserManager UserManager
        {
            get { return _userManager ?? (_userManager = OwinContext.GetUserManager<BackOfficeUserManager>()); }
        }

        /// <summary>
        /// Render the default view
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Default()
        {
            return await RenderDefaultOrProcessExternalLoginAsync(
                () => View(GlobalSettings.Path.EnsureEndsWith('/') + "Views/Default.cshtml"),
                () => View(GlobalSettings.Path.EnsureEndsWith('/') + "Views/Default.cshtml"));
        }

        /// <summary>
        /// This Action is used by the installer when an upgrade is detected but the admin user is not logged in. We need to 
        /// ensure the user is authenticated before the install takes place so we redirect here to show the standard login screen.
        /// </summary>
        /// <returns></returns>      
        [HttpGet]
        public async Task<ActionResult> AuthorizeUpgrade()
        {
            return await RenderDefaultOrProcessExternalLoginAsync(
                //The default view to render when there is no external login info or errors
                () => View(GlobalSettings.Path.EnsureEndsWith('/') + "Views/AuthorizeUpgrade.cshtml"),
                //The ActionResult to perform if external login is successful
                () => Redirect("/"));
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
                ? User.Identity.IsAuthenticated
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
            var gridConfig = UmbracoConfig.For.GridConfig(
                Logger,
                ApplicationContext.ApplicationCache.RuntimeCache,
                new DirectoryInfo(Server.MapPath(SystemDirectories.AppPlugins)),
                new DirectoryInfo(Server.MapPath(SystemDirectories.Config)),
                HttpContext.IsDebuggingEnabled);

            return new JsonNetResult { Data = gridConfig.EditorsConfig.Editors, Formatting = Formatting.Indented };
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
                        {"externalLoginsUrl", Url.Action("ExternalLogin", "BackOffice")},
                        {"externalLinkLoginsUrl", Url.Action("LinkLogin", "BackOffice")},
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
                            "relationApiBaseUrl", Url.GetUmbracoApiServiceBaseUrl<RelationController>(
                                controller => controller.GetById(0))
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
                {
                    "application", GetApplicationState()
                },
                {
                    "externalLogins", new Dictionary<string, object>
                    {
                        {
                            "providers", HttpContext.GetOwinContext().Authentication.GetExternalAuthenticationTypes()
                                .Where(p => p.Properties.ContainsKey("UmbracoBackOffice"))
                                .Select(p => new
                                {
                                    authType = p.AuthenticationType, caption = p.Caption,
                                    //TODO: Need to see if this exposes any sensitive data!
                                    properties = p.Properties
                                })
                                .ToArray()
                        }
                    }
                }
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
        
        [HttpPost]
        public ActionResult ExternalLogin(string provider, string redirectUrl = null)
        {
            if (redirectUrl == null)
            {
                redirectUrl = Url.Action("Default", "BackOffice");
            }

            // Request a redirect to the external login provider
            return new ChallengeResult(provider, redirectUrl);
        }

        [UmbracoAuthorize]
        [HttpPost]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider,
                Url.Action("ExternalLinkLoginCallback", "BackOffice"),
                User.Identity.GetUserId());
        }

        

        [HttpGet]
        public async Task<ActionResult> ExternalLinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(
                Core.Constants.Security.BackOfficeExternalAuthenticationType,
                XsrfKey, User.Identity.GetUserId());
            
            if (loginInfo == null)
            {
                //Add error and redirect for it to be displayed
                TempData["ExternalSignInError"] = new[] { "An error occurred, could not get external login info" };
                return RedirectToLocal(Url.Action("Default", "BackOffice"));
            }

            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId<int>(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToLocal(Url.Action("Default", "BackOffice"));
            }

            //Add errors and redirect for it to be displayed
            TempData["ExternalSignInError"] = result.Errors;
            return RedirectToLocal(Url.Action("Default", "BackOffice"));
        }

        /// <summary>
        /// Used by Default and AuthorizeUpgrade to render as per normal if there's no external login info, otherwise
        /// process the external login info.
        /// </summary>
        /// <returns></returns>
        private async Task<ActionResult> RenderDefaultOrProcessExternalLoginAsync(Func<ActionResult> defaultResponse, Func<ActionResult> externalSignInResponse)
        {
            if (defaultResponse == null) throw new ArgumentNullException("defaultResponse");
            if (externalSignInResponse == null) throw new ArgumentNullException("externalSignInResponse");

            ViewBag.UmbracoPath = GlobalSettings.UmbracoMvcArea;

            //check if there's errors in the TempData, assign to view bag and render the view
            if (TempData["ExternalSignInError"] != null)
            {
                ViewBag.ExternalSignInError = TempData["ExternalSignInError"];
                return defaultResponse();
            }

            //First check if there's external login info, if there's not proceed as normal
            var loginInfo = await OwinContext.Authentication.GetExternalLoginInfoAsync(
                Core.Constants.Security.BackOfficeExternalAuthenticationType);

            if (loginInfo == null)
            {
                return defaultResponse();
            }

            //we're just logging in with an external source, not linking accounts
            return await ExternalSignInAsync(loginInfo, externalSignInResponse);
        }

        private async Task<ActionResult> ExternalSignInAsync(ExternalLoginInfo loginInfo, Func<ActionResult> response)
        {
            if (loginInfo == null) throw new ArgumentNullException("loginInfo");
            if (response == null) throw new ArgumentNullException("response");

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                //TODO: It might be worth keeping some of the claims associated with the ExternalLoginInfo, in which case we 
                // wouldn't necessarily sign the user in here with the standard login, instead we'd update the 
                // UseUmbracoBackOfficeExternalCookieAuthentication extension method to have the correct provider and claims factory,
                // ticket format, etc.. to create our back office user including the claims assigned and in this method we'd just ensure 
                // that the ticket is created and stored and that the user is logged in.

                //sign in
                await SignInAsync(user, isPersistent: false);
            }
            else
            {
                if (await AutoLinkAndSignInExternalAccount(loginInfo) == false)
                {
                    ViewBag.ExternalSignInError = new[] { "The requested provider (" + loginInfo.Login.LoginProvider + ") has not been linked to to an account" };        
                }

                //Remove the cookie otherwise this message will keep appearing
                if (Response.Cookies[Core.Constants.Security.BackOfficeExternalCookieName] != null)
                {
                    Response.Cookies[Core.Constants.Security.BackOfficeExternalCookieName].Expires = DateTime.MinValue;    
                }
            }

            return response();
        }

        private async Task<bool> AutoLinkAndSignInExternalAccount(ExternalLoginInfo loginInfo)
        {
            //Here we can check if the provider associated with the request has been configured to allow
            // new users (auto-linked external accounts). This would never be used with public providers such as 
            // Google, unless you for some reason wanted anybody to be able to access the backend if they have a Google account
            // .... not likely! 

            var authType = OwinContext.Authentication.GetExternalAuthenticationTypes().FirstOrDefault(x => x.AuthenticationType == loginInfo.Login.LoginProvider);
            if (authType == null)
            {
                Logger.Warn<BackOfficeController>("Could not find external authentication provider registered: " + loginInfo.Login.LoginProvider);
                return false;
            }

            var autoLinkOptions = authType.GetExternalAuthenticationOptions();
            if (autoLinkOptions != null)
            {
                if (autoLinkOptions.ShouldAutoLinkExternalAccount(UmbracoContext, loginInfo))
                {
                    //we are allowing auto-linking/creating of local accounts
                    if (loginInfo.Email.IsNullOrWhiteSpace())
                    {
                        ViewBag.ExternalSignInError = new[] { "The requested provider (" + loginInfo.Login.LoginProvider + ") has not provided an email address, the account cannot be linked." };
                    }
                    else
                    {

                        //Now we need to perform the auto-link, so first we need to lookup/create a user with the email address
                        var foundByEmail = Services.UserService.GetByEmail(loginInfo.Email);
                        if (foundByEmail != null)
                        {
                            ViewBag.ExternalSignInError = new[] { "A user with this email address already exists locally. You will need to login locally to Umbraco and link this external provider: " + loginInfo.Login.LoginProvider };
                        }
                        else
                        {
                            var defaultUserType = autoLinkOptions.GetDefaultUserType(UmbracoContext, loginInfo);
                            var userType = Services.UserService.GetUserTypeByAlias(defaultUserType);
                            if (userType == null)
                            {
                                ViewBag.ExternalSignInError = new[] { "Could not auto-link this account, the specified User Type does not exist: " + defaultUserType };
                            }
                            else
                            {
                                //var userMembershipProvider = global::Umbraco.Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider();

                                var autoLinkUser = new BackOfficeIdentityUser()
                                {
                                    Email = loginInfo.Email,
                                    Name = loginInfo.ExternalIdentity.Name,
                                    UserTypeAlias = userType.Alias,
                                    AllowedSections = autoLinkOptions.GetDefaultAllowedSections(UmbracoContext, loginInfo),
                                    Culture = autoLinkOptions.GetDefaultCulture(UmbracoContext, loginInfo),
                                    UserName = loginInfo.Email
                                };
                                var userCreationResult = await UserManager.CreateAsync(autoLinkUser);

                                if (userCreationResult.Succeeded == false)
                                {
                                    ViewBag.ExternalSignInError = userCreationResult.Errors;
                                }
                                else
                                {
                                    var linkResult = await UserManager.AddLoginAsync(autoLinkUser.Id, loginInfo.Login);
                                    if (linkResult.Succeeded == false)
                                    {
                                        ViewBag.ExternalSignInError = linkResult.Errors;

                                        //If this fails, we should really delete the user since it will be in an inconsistent state!
                                        var deleteResult = await UserManager.DeleteAsync(autoLinkUser);
                                        if (deleteResult.Succeeded == false)
                                        {
                                            //DOH! ... this isn't good, combine all errors to be shown
                                            ViewBag.ExternalSignInError = linkResult.Errors.Concat(deleteResult.Errors);
                                        }
                                    }
                                    else
                                    {

                                        //Ok, we're all linked up! Assign the auto-link options to a ViewBag property, this can be used
                                        // in the view to render a custom view (AutoLinkExternalAccountView) if required, which will allow
                                        // a developer to display a custom angular view to prompt the user for more information if required.
                                        ViewBag.ExternalSignInAutoLinkOptions = autoLinkOptions;

                                        //sign in
                                        await SignInAsync(autoLinkUser, isPersistent: false);
                                    }
                                }
                            }
                        }

                    }
                }
                return true;
            }

            return false;
        }

        private async Task SignInAsync(BackOfficeIdentityUser user, bool isPersistent)
        {
            OwinContext.Authentication.SignOut(Core.Constants.Security.BackOfficeExternalAuthenticationType);

            var nowUtc = DateTime.Now.ToUniversalTime();

            OwinContext.Authentication.SignIn(
                new AuthenticationProperties()
                {
                    IsPersistent = isPersistent,
                    AllowRefresh = true,
                    IssuedUtc = nowUtc,
                    ExpiresUtc = nowUtc.AddMinutes(GlobalSettings.TimeOutInMinutes)
                },
                await user.GenerateUserIdentityAsync(UserManager));
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return OwinContext.Authentication; }
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

            var version = UmbracoVersion.GetSemanticVersion().ToString();

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
                        urlList.Add(jsFile);
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

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return Redirect("/");
        }

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri, string userId = null)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            private string LoginProvider { get; set; }
            private string RedirectUri { get; set; }
            private string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                //Ensure the forms auth module doesn't do a redirect!
                context.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;

                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri.EnsureEndsWith('/') };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
    }
}
