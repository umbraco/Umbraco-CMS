using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.UI;
using ClientDependency.Core.Config;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.Security.Identity;
using Umbraco.Web.Trees;
using Umbraco.Web.UI.JavaScript;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web.WebServices;
using Umbraco.Core.Services;
using Action = umbraco.BusinessLogic.Actions.Action;
using Constants = Umbraco.Core.Constants;

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
        private BackOfficeSignInManager _signInManager;

        private const string TokenExternalSignInError = "ExternalSignInError";
        private const string TokenPasswordResetCode = "PasswordResetCode";
        private static readonly string[] TempDataTokenNames = { TokenExternalSignInError, TokenPasswordResetCode };

        protected BackOfficeSignInManager SignInManager
        {
            get { return _signInManager ?? (_signInManager = OwinContext.Get<BackOfficeSignInManager>()); }
        }

        protected BackOfficeUserManager UserManager
        {
            get { return _userManager ?? (_userManager = OwinContext.GetUserManager<BackOfficeUserManager>()); }
        }

        protected IAuthenticationManager AuthenticationManager
        {
            get { return OwinContext.Authentication; }
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
                ? Security.IsAuthenticated()
                    //current culture is set at the very beginning of each request
                    ? Thread.CurrentThread.CurrentCulture
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
                    typeof(BackOfficeController) + "GetManifestAssetList",
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

        private string GetMaxRequestLength()
        {
            var section = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
            if (section == null) return string.Empty;
            return section.MaxRequestLength.ToString();
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
                            //TODO: Add 'umbracoApiControllerBaseUrl' which people can use in JS
                            // to prepend their URL. We could then also use this in our own resources instead of
                            // having each url defined here explicitly - we can do that in v8! for now
                            // for umbraco services we'll stick to explicitly defining the endpoints.

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
                            {
                                "disallowedUploadFiles",
                                string.Join(",", UmbracoConfig.For.UmbracoSettings().Content.DisallowedUploadFiles)
                            },
                            {
                                "maxFileSize",
                                GetMaxRequestLength()
                            },
                            {"keepUserLoggedIn", UmbracoConfig.For.UmbracoSettings().Security.KeepUserLoggedIn},
                            {"cssPath", IOHelper.ResolveUrl(SystemDirectories.Css).TrimEnd('/')},
                            {"allowPasswordReset", UmbracoConfig.For.UmbracoSettings().Security.AllowPasswordReset},
                        }
                    },
                    {
                        "umbracoPlugins", new Dictionary<string, object>
                        {
                            {"trees", GetTreePluginsMetaData()}
                        }
                    },
                    {
                        "isDebuggingEnabled", HttpContext.IsDebuggingEnabled
                    },
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
        public async Task<ActionResult> ValidatePasswordResetCode([Bind(Prefix = "u")]int userId, [Bind(Prefix = "r")]string resetCode)
        {
            var user = UserManager.FindById(userId);
            if (user != null)
            {
                var result = await UserManager.UserTokenProvider.ValidateAsync("ResetPassword", resetCode, UserManager, user);
                if (result)
                {
                    //Add a flag and redirect for it to be displayed
                    TempData[TokenPasswordResetCode] = new ValidatePasswordResetCodeModel {UserId = userId, ResetCode = resetCode};
                    return RedirectToLocal(Url.Action("Default", "BackOffice"));
                }
            }

            //Add error and redirect for it to be displayed
            TempData[TokenPasswordResetCode] = new[] { Services.TextService.Localize("login/resetCodeExpired") };
            return RedirectToLocal(Url.Action("Default", "BackOffice"));
        }

        [HttpGet]
        public async Task<ActionResult> ExternalLinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(
                Constants.Security.BackOfficeExternalAuthenticationType,
                XsrfKey, User.Identity.GetUserId());

            if (loginInfo == null)
            {
                //Add error and redirect for it to be displayed
                TempData[TokenExternalSignInError] = new[] { "An error occurred, could not get external login info" };
                return RedirectToLocal(Url.Action("Default", "BackOffice"));
            }

            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId<int>(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToLocal(Url.Action("Default", "BackOffice"));
            }

            //Add errors and redirect for it to be displayed
            TempData[TokenExternalSignInError] = result.Errors;
            return RedirectToLocal(Url.Action("Default", "BackOffice"));
        }

        /// <summary>
        /// Used by Default and AuthorizeUpgrade to render as per normal if there's no external login info, 
        /// otherwise process the external login info.
        /// </summary>
        /// <returns></returns>       
        private async Task<ActionResult> RenderDefaultOrProcessExternalLoginAsync(
            Func<ActionResult> defaultResponse, 
            Func<ActionResult> externalSignInResponse)
        {
            if (defaultResponse == null) throw new ArgumentNullException("defaultResponse");
            if (externalSignInResponse == null) throw new ArgumentNullException("externalSignInResponse");

            ViewBag.UmbracoPath = GlobalSettings.UmbracoMvcArea;

            //check if there is the TempData with the any token name specified, if so, assign to view bag and render the view
            foreach (var tempDataTokenName in TempDataTokenNames)
            {                
                if (TempData[tempDataTokenName] != null)
                {
                    ViewData[tempDataTokenName] = TempData[tempDataTokenName];
                    return defaultResponse();
                }
            }

            //First check if there's external login info, if there's not proceed as normal
            var loginInfo = await OwinContext.Authentication.GetExternalLoginInfoAsync(
                Constants.Security.BackOfficeExternalAuthenticationType);

            if (loginInfo == null || loginInfo.ExternalIdentity.IsAuthenticated == false)
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
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            else
            {
                if (await AutoLinkAndSignInExternalAccount(loginInfo) == false)
                {
                    ViewData[TokenExternalSignInError] = new[] { "The requested provider (" + loginInfo.Login.LoginProvider + ") has not been linked to to an account" };
                }

                //Remove the cookie otherwise this message will keep appearing
                if (Response.Cookies[Constants.Security.BackOfficeExternalCookieName] != null)
                {
                    Response.Cookies[Constants.Security.BackOfficeExternalCookieName].Expires = DateTime.MinValue;
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
                        ViewData[TokenExternalSignInError] = new[] { "The requested provider (" + loginInfo.Login.LoginProvider + ") has not provided an email address, the account cannot be linked." };
                    }
                    else
                    {

                        //Now we need to perform the auto-link, so first we need to lookup/create a user with the email address
                        var foundByEmail = Services.UserService.GetByEmail(loginInfo.Email);
                        if (foundByEmail != null)
                        {
                            ViewData[TokenExternalSignInError] = new[] { "A user with this email address already exists locally. You will need to login locally to Umbraco and link this external provider: " + loginInfo.Login.LoginProvider };
                        }
                        else
                        {
                            var defaultUserType = autoLinkOptions.GetDefaultUserType(UmbracoContext, loginInfo);
                            var userType = Services.UserService.GetUserTypeByAlias(defaultUserType);
                            if (userType == null)
                            {
                                ViewData[TokenExternalSignInError] = new[] { "Could not auto-link this account, the specified User Type does not exist: " + defaultUserType };
                            }
                            else
                            {

                                if (loginInfo.Email.IsNullOrWhiteSpace()) throw new InvalidOperationException("The Email value cannot be null");
                                if (loginInfo.ExternalIdentity.Name.IsNullOrWhiteSpace()) throw new InvalidOperationException("The Name value cannot be null");

                                var autoLinkUser = new BackOfficeIdentityUser()
                                {
                                    Email = loginInfo.Email,
                                    Name = loginInfo.ExternalIdentity.Name,
                                    UserTypeAlias = userType.Alias,
                                    AllowedSections = autoLinkOptions.GetDefaultAllowedSections(UmbracoContext, loginInfo),
                                    Culture = autoLinkOptions.GetDefaultCulture(UmbracoContext, loginInfo),
                                    UserName = loginInfo.Email
                                };

                                //call the callback if one is assigned
                                if (autoLinkOptions.OnAutoLinking != null)
                                {
                                    autoLinkOptions.OnAutoLinking(autoLinkUser, loginInfo);
                                }

                                var userCreationResult = await UserManager.CreateAsync(autoLinkUser);

                                if (userCreationResult.Succeeded == false)
                                {
                                    ViewData[TokenExternalSignInError] = userCreationResult.Errors;
                                }
                                else
                                {
                                    var linkResult = await UserManager.AddLoginAsync(autoLinkUser.Id, loginInfo.Login);
                                    if (linkResult.Succeeded == false)
                                    {
                                        ViewData[TokenExternalSignInError] = linkResult.Errors;

                                        //If this fails, we should really delete the user since it will be in an inconsistent state!
                                        var deleteResult = await UserManager.DeleteAsync(autoLinkUser);
                                        if (deleteResult.Succeeded == false)
                                        {
                                            //DOH! ... this isn't good, combine all errors to be shown
                                            ViewData[TokenExternalSignInError] = linkResult.Errors.Concat(deleteResult.Errors);
                                        }
                                    }
                                    else
                                    {
                                        //sign in
                                        await SignInManager.SignInAsync(autoLinkUser, isPersistent: false, rememberBrowser: false);
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

            var version = UmbracoVersion.GetSemanticVersion().ToSemanticString();

            app.Add("version", version);
            app.Add("cdf", ClientDependencySettings.Instance.Version);
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
                tree = x,
                attributes =
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

        internal static IEnumerable<string> GetLegacyActionJsForActions(LegacyJsActionType type, IEnumerable<string> values)
        {
            var blockList = new List<string>();
            var urlList = new List<string>();
            foreach (var jsFile in values)
            {
                var isJsPath = jsFile.DetectIsJavaScriptPath();
                if (isJsPath.Success)

                {
                    urlList.Add(isJsPath.Result);
                }
                else
                {
                    blockList.Add(isJsPath.Result);
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

        /// <summary>
        /// Renders out all JavaScript references that have bee declared in IActions
        /// </summary>
        private static IEnumerable<string> GetLegacyActionJs(LegacyJsActionType type)
        {
            return GetLegacyActionJsForActions(type, Action.GetJavaScriptFileReferences());
        }

        internal enum LegacyJsActionType
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

                var owinCtx = context.HttpContext.GetOwinContext();

                //First, see if a custom challenge result callback is specified for the provider
                // and use it instead of the default if one is supplied.
                var loginProvider = owinCtx.Authentication
                    .GetExternalAuthenticationTypes()
                    .FirstOrDefault(p => p.AuthenticationType == LoginProvider);
                if (loginProvider != null)
                {
                    var providerChallengeResult = loginProvider.GetSignInChallengeResult(owinCtx);
                    if (providerChallengeResult != null)
                    {
                        owinCtx.Authentication.Challenge(providerChallengeResult, LoginProvider);
                        return;
                    }
                }

                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri.EnsureEndsWith('/') };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                owinCtx.Authentication.Challenge(properties, LoginProvider);
            }
        }
    }
}
