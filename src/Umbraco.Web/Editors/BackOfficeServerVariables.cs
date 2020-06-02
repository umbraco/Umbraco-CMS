﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Features;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.WebAssets;
using Umbraco.Web.Security;

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
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IContentSettings _contentSettings;
        private readonly TreeCollection _treeCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRuntimeSettings _settings;
        private readonly ISecuritySettings _securitySettings;
        private readonly IRuntimeMinifier _runtimeMinifier;

        internal BackOfficeServerVariables(
            UrlHelper urlHelper,
            IRuntimeState runtimeState,
            UmbracoFeatures features,
            IGlobalSettings globalSettings,
            IUmbracoVersion umbracoVersion,
            IContentSettings contentSettings,
            TreeCollection treeCollection,
            IHostingEnvironment hostingEnvironment,
            IRuntimeSettings settings,
            ISecuritySettings securitySettings,
            IRuntimeMinifier runtimeMinifier)
        {
            _urlHelper = urlHelper;
            _runtimeState = runtimeState;
            _features = features;
            _globalSettings = globalSettings;
            _umbracoVersion = umbracoVersion;
            _contentSettings = contentSettings ?? throw new ArgumentNullException(nameof(contentSettings));
            _treeCollection = treeCollection ?? throw new ArgumentNullException(nameof(treeCollection));
            _hostingEnvironment = hostingEnvironment;
            _settings = settings;
            _securitySettings = securitySettings;
            _runtimeMinifier = runtimeMinifier;
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
                {"umbracoUrls", new[] {"authenticationApiBaseUrl", "serverVarsJs", "externalLoginsUrl", "currentUserApiBaseUrl"}},
                {"umbracoSettings", new[] {"allowPasswordReset", "imageFileTypes", "maxFileSize", "loginBackgroundImage", "canSendRequiredEmail", "usernameIsEmail"}},
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
            var globalSettings = _globalSettings;
            var defaultVals = new Dictionary<string, object>
            {
                {
                    "umbracoUrls", new Dictionary<string, object>
                    {
                        // TODO: Add 'umbracoApiControllerBaseUrl' which people can use in JS
                        // to prepend their URL. We could then also use this in our own resources instead of
                        // having each url defined here explicitly - we can do that in v8! for now
                        // for umbraco services we'll stick to explicitly defining the endpoints.

                        {"externalLoginsUrl", _urlHelper.Action("ExternalLogin", "BackOffice")},
                        {"externalLinkLoginsUrl", _urlHelper.Action("LinkLogin", "BackOffice")},
                        {"gridConfig", _urlHelper.Action("GetGridConfig", "BackOffice")},
                        // TODO: This is ultra confusing! this same key is used for different things, when returning the full app when authenticated it is this URL but when not auth'd it's actually the ServerVariables address
                        {"serverVarsJs", _urlHelper.Action("Application", "BackOffice")},
                        //API URLs
                        {
                            "packagesRestApiBaseUrl", Constants.PackageRepository.RestApiBaseUrl
                        },
                        // {
                        //     "redirectUrlManagementApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<RedirectUrlManagementController>(
                        //         controller => controller.GetEnableState())
                        // },
                        //TODO reintroduce
                        // {
                        //     "tourApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<TourController>(
                        //         controller => controller.GetTours())
                        // },
                        //TODO reintroduce
                        // {
                        //     "embedApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<RteEmbedController>(
                        //         controller => controller.GetEmbed("", 0, 0))
                        // },
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
                        //TODO reintroduce
                        // {
                        //     "imagesApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<ImagesController>(
                        //         controller => controller.GetBigThumbnail(""))
                        // },
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
                        // {
                        //     "dataTypeApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<DataTypeController>(
                        //         controller => controller.GetById(0))
                        // },
                        //TODO Reintroduce
                        // {
                        //     "dashboardApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<DashboardController>(
                        //         controller => controller.GetDashboard(null))
                        // },
                        // {
                        //     "logApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<LogController>(
                        //         controller => controller.GetPagedEntityLog(0, 0, 0, Direction.Ascending, null))
                        // },
                        {
                            "memberApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<MemberController>(
                                controller => controller.GetByKey(Guid.Empty))
                        },
                        // {
                        //     "packageInstallApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<PackageInstallController>(
                        //         controller => controller.Fetch(string.Empty))
                        // },
                        // {
                        //     "packageApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<PackageController>(
                        //         controller => controller.GetCreatedPackages())
                        // },
                        // {
                        //     "relationApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<RelationController>(
                        //         controller => controller.GetById(0))
                        // },
                        // {
                        //     "rteApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<RichTextPreValueController>(
                        //         controller => controller.GetConfiguration())
                        // },
                        // {
                        //     "stylesheetApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<StylesheetController>(
                        //         controller => controller.GetAll())
                        // },
                        {
                            "memberTypeApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<MemberTypeController>(
                                controller => controller.GetAllTypes())
                        },
                        {
                            "memberGroupApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<MemberGroupController>(
                                controller => controller.GetAllGroups())
                        },
                        // {
                        //     "updateCheckApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<UpdateCheckController>(
                        //         controller => controller.GetCheck())
                        // },
                        // {
                        //     "templateApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<TemplateController>(
                        //         controller => controller.GetById(0))
                        // },
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
                        // {
                        //     "tagsDataBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<TagsDataController>(
                        //         controller => controller.GetTags("", "", null))
                        // },
                        //TODO reintroduce
                        // {
                        //     "examineMgmtBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<ExamineManagementController>(
                        //         controller => controller.GetIndexerDetails())
                        // },
                        // {
                        //     "healthCheckBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<HealthCheckController>(
                        //         controller => controller.GetAllHealthChecks())
                        // },
                        {
                            "templateQueryApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<TemplateQueryController>(
                                controller => controller.PostTemplateQuery(null))
                        },
                        //TODO Reintroduce
                        // {
                        //     "codeFileApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<CodeFileController>(
                        //         controller => controller.GetByPath("", ""))
                        // },
                        // {
                        //     "publishedStatusBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<PublishedStatusController>(
                        //         controller => controller.GetPublishedStatusUrl())
                        // },
      //                   {
      //                       "dictionaryApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<DictionaryController>(
      //                           controller => controller.DeleteById(int.MaxValue))
						// },
                        // {
                        //     "publishedSnapshotCacheStatusBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<PublishedSnapshotCacheStatusController>(
                        //         controller => controller.GetStatus())
                        // },
                        //TODO Reintroduce
                        // {
                        //     "helpApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<HelpController>(
                        //         controller => controller.GetContextHelpForPage("","",""))
                        // },
                        //TODO Reintroduce
                        // {
                        //     "backOfficeAssetsApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<BackOfficeAssetsController>(
                        //         controller => controller.GetSupportedLocales())
                        // },
                        // {
                        //     "languageApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<LanguageController>(
                        //         controller => controller.GetAllLanguages())
                        // },
          //               {
						    // "relationTypeApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<RelationTypeController>(
          //                       controller => controller.GetById(1))
          //               },
						// {
      //                       "logViewerApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<LogViewerController>(
      //                           controller => controller.GetNumberOfErrors(null, null))
      //                   },
                        // {
                        //     "webProfilingBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<WebProfilingController>(
                        //         controller => controller.GetStatus())
                        // },
                        // {
                        //     "tinyMceApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<TinyMceController>(
                        //         controller => controller.UploadImage())
                        // },
                        //TODO Reintroduce
                        // {
                        //     "imageUrlGeneratorApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<ImageUrlGeneratorController>(
                        //         controller => controller.GetCropUrl(null, null, null, null, null))
                        // },
                    }
                },
                {
                    "umbracoSettings", new Dictionary<string, object>
                    {
                        {"umbracoPath", _globalSettings.GetBackOfficePath(_hostingEnvironment)},
                        {"mediaPath", _hostingEnvironment.ToAbsolute(globalSettings.UmbracoMediaPath).TrimEnd('/')},
                        {"appPluginsPath", _hostingEnvironment.ToAbsolute(Constants.SystemDirectories.AppPlugins).TrimEnd('/')},
                        {
                            "imageFileTypes",
                            string.Join(",", _contentSettings.ImageFileTypes)
                        },
                        {
                            "disallowedUploadFiles",
                            string.Join(",", _contentSettings.DisallowedUploadFiles)
                        },
                        {
                            "allowedUploadFiles",
                            string.Join(",", _contentSettings.AllowedUploadFiles)
                        },
                        {
                            "maxFileSize",
                            GetMaxRequestLength()
                        },
                        {"keepUserLoggedIn", _securitySettings.KeepUserLoggedIn},
                        {"usernameIsEmail", _securitySettings.UsernameIsEmail},
                        {"cssPath", _hostingEnvironment.ToAbsolute(globalSettings.UmbracoCssPath).TrimEnd('/')},
                        {"allowPasswordReset", _securitySettings.AllowPasswordReset},
                        {"loginBackgroundImage", _contentSettings.LoginBackgroundImage},
                        {"showUserInvite", EmailSender.CanSendRequiredEmail(globalSettings)},
                        {"canSendRequiredEmail", EmailSender.CanSendRequiredEmail(globalSettings)},
                        {"showAllowSegmentationForDocumentTypes", false},
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
                    "isDebuggingEnabled", _hostingEnvironment.IsDebugMode
                },
                {
                    "application", GetApplicationState()
                },
                {
                    "externalLogins", new Dictionary<string, object>
                    {
                        {
                            "providers", _httpContextAccessor.GetRequiredHttpContext().GetOwinContext().Authentication.GetExternalAuthenticationTypes()
                                .Where(p => p.Properties.ContainsKey("UmbracoBackOffice"))
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

            foreach (var tree in _treeCollection)
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
                { "version", _umbracoVersion.SemanticVersion.ToSemanticString() },

                // the assembly version (eg "8.0.0")
                { "assemblyVersion", _umbracoVersion.AssemblyVersion.ToString() }
            };

            var version = _runtimeState.SemanticVersion.ToSemanticString();

            //the value is the hash of the version, cdf version and the configured state
            app.Add("cacheBuster", $"{version}.{_runtimeState.Level}.{_runtimeMinifier.CacheBuster}".GenerateHash());

            //useful for dealing with virtual paths on the client side when hosted in virtual directories especially
            app.Add("applicationPath", _httpContextAccessor.GetRequiredHttpContext().Request.ApplicationPath.EnsureEndsWith('/'));

            //add the server's GMT time offset in minutes
            app.Add("serverTimeOffset", Convert.ToInt32(DateTimeOffset.Now.Offset.TotalMinutes));

            return app;
        }

        private string GetMaxRequestLength()
        {
            return _settings.MaxRequestLength.HasValue ? _settings.MaxRequestLength.Value.ToString() : string.Empty;
        }
    }
}
