using ClientDependency.Core.Config;
using Microsoft.Owin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Web.Features;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.Profiling;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.Security;
using Umbraco.Web.Trees;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Used to collect the server variables for use in the back office angular app
    /// </summary>
    internal class BackOfficeServerVariables
    {
        private readonly UrlHelper _urlHelper;
        private readonly IRuntimeState _runtimeState;
        private readonly UmbracoFeatures _features;
        private readonly IGlobalSettings _globalSettings;
        private readonly HttpContextBase _httpContext;
        private readonly IOwinContext _owinContext;

        internal BackOfficeServerVariables(UrlHelper urlHelper, IRuntimeState runtimeState, UmbracoFeatures features, IGlobalSettings globalSettings)
        {
            _urlHelper = urlHelper;
            _runtimeState = runtimeState;
            _features = features;
            _globalSettings = globalSettings;
            _httpContext = _urlHelper.RequestContext.HttpContext;
            _owinContext = _httpContext.GetOwinContext();
        }

        /// <summary>
        /// Returns the server variables for non-authenticated users
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, object> BareMinimumServerVariables()
        {
            //this is the filter for the keys that we'll keep based on the full version of the server vars
            var keepOnlyKeys = new Dictionary<string, string[]>
            {
                {"umbracoUrls", new[] {"authenticationApiBaseUrl", "serverVarsJs", "externalLoginsUrl", "currentUserApiBaseUrl", "iconApiBaseUrl"}},
                {"umbracoSettings", new[] {"allowPasswordReset", "imageFileTypes", "maxFileSize", "loginBackgroundImage", "loginLogoImage", "canSendRequiredEmail", "usernameIsEmail", "minimumPasswordLength", "minimumPasswordNonAlphaNum", "hideBackofficeLogo"}},
                {"application", new[] {"applicationPath", "cacheBuster"}},
                {"isDebuggingEnabled", new string[] { }},
                {"features", new [] {"disabledFeatures"}}
            };
            //now do the filtering...
            var defaults = GetServerVariables();
            foreach (var key in defaults.Keys.ToArray())
            {
                if (keepOnlyKeys.ContainsKey(key) == false)
                {
                    defaults.Remove(key);
                }
                else
                {
                    var asDictionary = defaults[key] as IDictionary;
                    if (asDictionary != null)
                    {
                        var toKeep = keepOnlyKeys[key];
                        foreach (var k in asDictionary.Keys.Cast<string>().ToArray())
                        {
                            if (toKeep.Contains(k) == false)
                            {
                                asDictionary.Remove(k);
                            }
                        }
                    }
                }
            }

            // TODO: This is ultra confusing! this same key is used for different things, when returning the full app when authenticated it is this URL but when not auth'd it's actually the ServerVariables address
            // so based on compat and how things are currently working we need to replace the serverVarsJs one
            ((Dictionary<string, object>)defaults["umbracoUrls"])["serverVarsJs"] = _urlHelper.Action("ServerVariables", "BackOffice");

            return defaults;
        }

        /// <summary>
        /// Returns the server variables for authenticated users
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, object> GetServerVariables()
        {
            var userMembershipProvider = Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider();

            var defaultVals = new Dictionary<string, object>
            {
                {
                    "umbracoUrls", new Dictionary<string, object>
                    {
                        // TODO: Add 'umbracoApiControllerBaseUrl' which people can use in JS
                        // to prepend their URL. We could then also use this in our own resources instead of
                        // having each URL defined here explicitly - we can do that in v8! for now
                        // for umbraco services we'll stick to explicitly defining the endpoints.

                        {"externalLoginsUrl", _urlHelper.Action("ExternalLogin", "BackOffice")},
                        {"externalLinkLoginsUrl", _urlHelper.Action("LinkLogin", "BackOffice")},
                        {"manifestAssetList", _urlHelper.Action("GetManifestAssetList", "BackOffice")},
                        {"gridConfig", _urlHelper.Action("GetGridConfig", "BackOffice")},
                        // TODO: This is ultra confusing! this same key is used for different things, when returning the full app when authenticated it is this URL but when not auth'd it's actually the ServerVariables address
                        {"serverVarsJs", _urlHelper.Action("Application", "BackOffice")},
                        //API URLs
                        {
                            "packagesRestApiBaseUrl", Constants.PackageRepository.RestApiBaseUrl
                        },
                        {
                            "redirectUrlManagementApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<RedirectUrlManagementController>(
                                controller => controller.GetEnableState())
                        },
                        {
                            "tourApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<TourController>(
                                controller => controller.GetTours())
                        },
                        {
                            "embedApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<RteEmbedController>(
                                controller => controller.GetEmbed("", 0, 0))
                        },
                        {
                            "userApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<UsersController>(
                                controller => controller.PostSaveUser(null))
                        },
                        {
                            "userGroupsApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<UserGroupsController>(
                                controller => controller.PostSaveUserGroup(null))
                        },
                        {
                            "contentApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<ContentController>(
                                controller => controller.PostSave(null))
                        },
                        {
                            "mediaApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<MediaController>(
                                controller => controller.GetRootMedia())
                        },
                        {
                            "imagesApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<ImagesController>(
                                controller => controller.GetBigThumbnail(""))
                        },
                        {
                            "sectionApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<SectionController>(
                                controller => controller.GetSections())
                        },
                        {
                            "treeApplicationApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<ApplicationTreeController>(
                                controller => controller.GetApplicationTrees(null, null, null, TreeUse.None))
                        },
                        {
                            "contentTypeApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<ContentTypeController>(
                                controller => controller.GetAllowedChildren(0))
                        },
                        {
                            "mediaTypeApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<MediaTypeController>(
                                controller => controller.GetAllowedChildren(0))
                        },
                        {
                            "macroRenderingApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<MacroRenderingController>(
                                controller => controller.GetMacroParameters(0))
                        },
                        {
                            "macroApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<MacrosController>(
                                controller => controller.Create(null))
                        },
                        {
                            "authenticationApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<AuthenticationController>(
                                controller => controller.PostLogin(null))
                        },
                        {
                            "currentUserApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<CurrentUserController>(
                                controller => controller.PostChangePassword(null))
                        },
                        {
                            "entityApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<EntityController>(
                                controller => controller.GetById(0, UmbracoEntityTypes.Media))
                        },
                        {
                            "dataTypeApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<DataTypeController>(
                                controller => controller.GetById(0))
                        },
                        {
                            "dashboardApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<DashboardController>(
                                controller => controller.GetDashboard(null))
                        },
                        {
                            "logApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<LogController>(
                                controller => controller.GetPagedEntityLog(0, 0, 0, Core.Persistence.DatabaseModelDefinitions.Direction.Ascending, null))
                        },
                        {
                            "memberApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<MemberController>(
                                controller => controller.GetByKey(Guid.Empty))
                        },
                        {
                            "packageInstallApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<PackageInstallController>(
                                controller => controller.Fetch(string.Empty))
                        },
                        {
                            "packageApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<PackageController>(
                                controller => controller.GetCreatedPackages())
                        },
                        {
                            "relationApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<RelationController>(
                                controller => controller.GetById(0))
                        },
                        {
                            "rteApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<RichTextPreValueController>(
                                controller => controller.GetConfiguration())
                        },
                        {
                            "stylesheetApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<StylesheetController>(
                                controller => controller.GetAll())
                        },
                        {
                            "memberTypeApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<MemberTypeController>(
                                controller => controller.GetAllTypes())
                        },
                        {
                            "memberGroupApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<MemberGroupController>(
                                controller => controller.GetAllGroups())
                        },
                        {
                            "updateCheckApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<UpdateCheckController>(
                                controller => controller.GetCheck())
                        },
                        {
                            "templateApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<TemplateController>(
                                controller => controller.GetById(0))
                        },
                        {
                            "memberTreeBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<MemberTreeController>(
                                controller => controller.GetNodes("-1", null))
                        },
                        {
                            "mediaTreeBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<MediaTreeController>(
                                controller => controller.GetNodes("-1", null))
                        },
                        {
                            "contentTreeBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<ContentTreeController>(
                                controller => controller.GetNodes("-1", null))
                        },
                        {
                            "tagsDataBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<TagsDataController>(
                                controller => controller.GetTags("", "", null))
                        },
                        {
                            "examineMgmtBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<ExamineManagementController>(
                                controller => controller.GetIndexerDetails())
                        },
                        {
                            "healthCheckBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<HealthCheckController>(
                                controller => controller.GetAllHealthChecks())
                        },
                        {
                            "templateQueryApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<TemplateQueryController>(
                                controller => controller.PostTemplateQuery(null))
                        },
                        {
                            "codeFileApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<CodeFileController>(
                                controller => controller.GetByPath("", ""))
                        },
                        {
                            "publishedStatusBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<PublishedStatusController>(
                                controller => controller.GetPublishedStatusUrl())
                        },
                        {
                            "dictionaryApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<DictionaryController>(
                                controller => controller.DeleteById(int.MaxValue))
                        },
                        {
                            "nuCacheStatusBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<NuCacheStatusController>(
                                controller => controller.GetStatus())
                        },
                        {
                            "helpApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<HelpController>(
                                controller => controller.GetContextHelpForPage("","",""))
                        },
                        {
                            "backOfficeAssetsApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<BackOfficeAssetsController>(
                                controller => controller.GetSupportedLocales())
                        },
                        {
                            "languageApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<LanguageController>(
                                controller => controller.GetAllLanguages())
                        },
                        {
                            "relationTypeApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<RelationTypeController>(
                                controller => controller.GetById(1))
                        },
                        {
                            "logViewerApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<LogViewerController>(
                                controller => controller.GetNumberOfErrors(null, null))
                        },
                        {
                            "iconApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<IconController>(
                                controller => controller.GetIcon(""))
                        },
                        {
                            "webProfilingBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<WebProfilingController>(
                                controller => controller.GetStatus())
                        },
                        {
                            "tinyMceApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<TinyMceController>(
                                controller => controller.UploadImage())
                        },
                        {
                            "imageUrlGeneratorApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<ImageUrlGeneratorController>(
                                controller => controller.GetCropUrl(null, null, null, null, null))
                        },
                        {
                            "elementTypeApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<ElementTypeController>(
                                controller => controller.GetAll())
                        },
                    }
                },
                {
                    "umbracoSettings", new Dictionary<string, object>
                    {
                        {"umbracoPath", _globalSettings.Path},
                        {"mediaPath", IOHelper.ResolveUrl(SystemDirectories.Media).TrimEnd(Constants.CharArrays.ForwardSlash)},
                        {"appPluginsPath", IOHelper.ResolveUrl(SystemDirectories.AppPlugins).TrimEnd(Constants.CharArrays.ForwardSlash)},
                        {
                            "imageFileTypes",
                            string.Join(",", Current.Configs.Settings().Content.ImageFileTypes)
                        },
                        {
                            "disallowedUploadFiles",
                            string.Join(",", Current.Configs.Settings().Content.DisallowedUploadFiles)
                        },
                        {
                            "allowedUploadFiles",
                            string.Join(",", Current.Configs.Settings().Content.AllowedUploadFiles)
                        },
                        {
                            "maxFileSize",
                            GetMaxRequestLength()
                        },
                        {"keepUserLoggedIn", Current.Configs.Settings().Security.KeepUserLoggedIn},
                        {"usernameIsEmail", Current.Configs.Settings().Security.UsernameIsEmail},
                        {"cssPath", IOHelper.ResolveUrl(SystemDirectories.Css).TrimEnd(Constants.CharArrays.ForwardSlash)},
                        {"allowPasswordReset", Current.Configs.Settings().Security.AllowPasswordReset},
                        {"loginBackgroundImage",  Current.Configs.Settings().Content.LoginBackgroundImage},
                        {"loginLogoImage", Current.Configs.Settings().Content.LoginLogoImage },
                        {"hideBackofficeLogo", Current.Configs.Settings().Content.HideBackOfficeLogo },
                        {"showUserInvite", EmailSender.CanSendRequiredEmail},
                        {"canSendRequiredEmail", EmailSender.CanSendRequiredEmail},
                        {"showAllowSegmentationForDocumentTypes", false},
                        {"minimumPasswordLength", userMembershipProvider.MinRequiredPasswordLength},
                        {"minimumPasswordNonAlphaNum", userMembershipProvider.MinRequiredNonAlphanumericCharacters},
                        {"sanitizeTinyMce", Current.Configs.Global().SanitizeTinyMce}
                    }
                },
                {
                    "umbracoPlugins", new Dictionary<string, object>
                    {
                        // for each tree that is [PluginController], get
                        // alias -> areaName
                        // so that routing (route.js) can look for views
                        { "trees", GetPluginTrees().ToArray() }
                    }
                },
                {
                    "isDebuggingEnabled", _httpContext.IsDebuggingEnabled
                },
                {
                    "application", GetApplicationState()
                },
                {
                    "externalLogins", new Dictionary<string, object>
                    {
                        {
                            "providers", _owinContext.Authentication.GetBackOfficeExternalLoginProviders()
                                .Select(p => new
                                {
                                    authType = p.AuthenticationType, caption = p.Caption,
                                    // TODO: Need to see if this exposes any sensitive data!
                                    properties = p.Properties
                                })
                                .ToArray()
                        }
                    }
                },
                {
                    "features", new Dictionary<string,object>
                    {
                        {
                            "disabledFeatures", new Dictionary<string,object>
                            {
                                { "disableTemplates", _features.Disabled.DisableTemplates}
                            }
                        }

                    }
                }
            };
            return defaultVals;
        }

        [DataContract]
        private class PluginTree
        {
            [DataMember(Name = "alias")]
            public string Alias { get; set; }

            [DataMember(Name = "packageFolder")]
            public string PackageFolder { get; set; }
        }

        private IEnumerable<PluginTree> GetPluginTrees()
        {
            // used to be (cached)
            //var treeTypes = Current.TypeLoader.GetAttributedTreeControllers();
            //
            // ie inheriting from TreeController and marked with TreeAttribute
            //
            // do this instead
            // inheriting from TreeControllerBase and marked with TreeAttribute
            var trees = Current.Factory.GetInstance<TreeCollection>();

            foreach (var tree in trees)
            {
                var treeType = tree.TreeControllerType;

                // exclude anything marked with CoreTreeAttribute
                var coreTree = treeType.GetCustomAttribute<CoreTreeAttribute>(false);
                if (coreTree != null) continue;

                // exclude anything not marked with PluginControllerAttribute
                var pluginController = treeType.GetCustomAttribute<PluginControllerAttribute>(false);
                if (pluginController == null) continue;

                yield return new PluginTree { Alias = tree.TreeAlias, PackageFolder = pluginController.AreaName };
            }
        }

        /// <summary>
        /// Returns the server variables regarding the application state
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetApplicationState()
        {
            var app = new Dictionary<string, object>
            {
                // add versions - see UmbracoVersion for details & differences

                // the complete application version (eg "8.1.2-alpha.25")
                { "version", UmbracoVersion.SemanticVersion.ToSemanticString() },

                // the assembly version (eg "8.0.0")
                { "assemblyVersion", UmbracoVersion.AssemblyVersion.ToString() }
            };

            var version = _runtimeState.SemanticVersion.ToSemanticString();

            //the value is the hash of the version, cdf version and the configured state
            app.Add("cacheBuster", $"{version}.{_runtimeState.Level}.{ClientDependencySettings.Instance.Version}".GenerateHash());

            //useful for dealing with virtual paths on the client side when hosted in virtual directories especially
            app.Add("applicationPath", _httpContext.Request.ApplicationPath.EnsureEndsWith('/'));

            //add the server's GMT time offset in minutes
            app.Add("serverTimeOffset", Convert.ToInt32(DateTimeOffset.Now.Offset.TotalMinutes));

            return app;
        }

        private static string GetMaxRequestLength()
        {
            return ConfigurationManager.GetSection("system.web/httpRuntime") is HttpRuntimeSection section
                ? section.MaxRequestLength.ToString()
                : string.Empty;
        }
    }
}
