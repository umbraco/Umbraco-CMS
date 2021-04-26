using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Web.BackOffice.HealthChecks;
using Umbraco.Cms.Web.BackOffice.Profiling;
using Umbraco.Cms.Web.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.BackOffice.Routing;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    /// <summary>
    /// Used to collect the server variables for use in the back office angular app
    /// </summary>
    public class BackOfficeServerVariables
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IRuntimeState _runtimeState;
        private readonly UmbracoFeatures _features;
        private readonly GlobalSettings _globalSettings;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly ContentSettings _contentSettings;
        private readonly TreeCollection _treeCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly RuntimeSettings _runtimeSettings;
        private readonly SecuritySettings _securitySettings;
        private readonly IRuntimeMinifier _runtimeMinifier;
        private readonly IBackOfficeExternalLoginProviders _externalLogins;
        private readonly IImageUrlGenerator _imageUrlGenerator;
        private readonly PreviewRoutes _previewRoutes;
        private readonly IEmailSender _emailSender;
        private readonly MemberPasswordConfigurationSettings _memberPasswordConfigurationSettings;

        public BackOfficeServerVariables(
            LinkGenerator linkGenerator,
            IRuntimeState runtimeState,
            UmbracoFeatures features,
            IOptions<GlobalSettings> globalSettings,
            IUmbracoVersion umbracoVersion,
            IOptions<ContentSettings> contentSettings,
            IHttpContextAccessor httpContextAccessor,
            TreeCollection treeCollection,
            IHostingEnvironment hostingEnvironment,
            IOptions<RuntimeSettings> runtimeSettings,
            IOptions<SecuritySettings> securitySettings,
            IRuntimeMinifier runtimeMinifier,
            IBackOfficeExternalLoginProviders externalLogins,
            IImageUrlGenerator imageUrlGenerator,
            PreviewRoutes previewRoutes,
            IEmailSender emailSender,
            IOptions<MemberPasswordConfigurationSettings> memberPasswordConfigurationSettings)
        {
            _linkGenerator = linkGenerator;
            _runtimeState = runtimeState;
            _features = features;
            _globalSettings = globalSettings.Value;
            _umbracoVersion = umbracoVersion;
            _contentSettings = contentSettings.Value ?? throw new ArgumentNullException(nameof(contentSettings));
            _httpContextAccessor = httpContextAccessor;
            _treeCollection = treeCollection ?? throw new ArgumentNullException(nameof(treeCollection));
            _hostingEnvironment = hostingEnvironment;
            _runtimeSettings = runtimeSettings.Value;
            _securitySettings = securitySettings.Value;
            _runtimeMinifier = runtimeMinifier;
            _externalLogins = externalLogins;
            _imageUrlGenerator = imageUrlGenerator;
            _previewRoutes = previewRoutes;
            _emailSender = emailSender;
            _memberPasswordConfigurationSettings = memberPasswordConfigurationSettings.Value;
        }

        /// <summary>
        /// Returns the server variables for non-authenticated users
        /// </summary>
        /// <returns></returns>
        internal async Task<Dictionary<string, object>> BareMinimumServerVariablesAsync()
        {
            //this is the filter for the keys that we'll keep based on the full version of the server vars
            var keepOnlyKeys = new Dictionary<string, string[]>
            {
                {"umbracoUrls", new[] {"authenticationApiBaseUrl", "serverVarsJs", "externalLoginsUrl", "currentUserApiBaseUrl", "previewHubUrl", "iconApiBaseUrl"}},
                {"umbracoSettings", new[] {"allowPasswordReset", "imageFileTypes", "maxFileSize", "loginBackgroundImage", "loginLogoImage", "canSendRequiredEmail", "usernameIsEmail", "minimumPasswordLength", "minimumPasswordNonAlphaNum"}},
                {"application", new[] {"applicationPath", "cacheBuster"}},
                {"isDebuggingEnabled", new string[] { }},
                {"features", new [] {"disabledFeatures"}}
            };
            //now do the filtering...
            var defaults = await GetServerVariablesAsync();
            foreach (var key in defaults.Keys.ToArray())
            {
                if (keepOnlyKeys.ContainsKey(key) == false)
                {
                    defaults.Remove(key);
                }
                else
                {
                    if (defaults[key] is System.Collections.IDictionary asDictionary)
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
            ((Dictionary<string, object>)defaults["umbracoUrls"])["serverVarsJs"]
                = _linkGenerator.GetPathByAction(
                    nameof(BackOfficeController.ServerVariables),
                    ControllerExtensions.GetControllerName<BackOfficeController>(),
                    new { area = Constants.Web.Mvc.BackOfficeArea });

            return defaults;
        }

        /// <summary>
        /// Returns the server variables for authenticated users
        /// </summary>
        /// <returns></returns>
        internal Task<Dictionary<string, object>> GetServerVariablesAsync()
        {
            var globalSettings = _globalSettings;
            var backOfficeControllerName = ControllerExtensions.GetControllerName<BackOfficeController>();
            var defaultVals = new Dictionary<string, object>
            {
                {
                    "umbracoUrls", new Dictionary<string, object>
                    {
                        // TODO: Add 'umbracoApiControllerBaseUrl' which people can use in JS
                        // to prepend their URL. We could then also use this in our own resources instead of
                        // having each URL defined here explicitly - we can do that in v8! for now
                        // for umbraco services we'll stick to explicitly defining the endpoints.

                        {"externalLoginsUrl", _linkGenerator.GetPathByAction(nameof(BackOfficeController.ExternalLogin), backOfficeControllerName, new { area = Constants.Web.Mvc.BackOfficeArea })},
                        {"externalLinkLoginsUrl", _linkGenerator.GetPathByAction(nameof(BackOfficeController.LinkLogin), backOfficeControllerName, new { area = Constants.Web.Mvc.BackOfficeArea })},
                        {"gridConfig", _linkGenerator.GetPathByAction(nameof(BackOfficeController.GetGridConfig), backOfficeControllerName, new { area = Constants.Web.Mvc.BackOfficeArea })},
                        // TODO: This is ultra confusing! this same key is used for different things, when returning the full app when authenticated it is this URL but when not auth'd it's actually the ServerVariables address
                        {"serverVarsJs", _linkGenerator.GetPathByAction(nameof(BackOfficeController.Application), backOfficeControllerName, new { area = Constants.Web.Mvc.BackOfficeArea })},
                        //API URLs
                        {
                            "packagesRestApiBaseUrl", Constants.PackageRepository.RestApiBaseUrl
                        },
                        {
                            "redirectUrlManagementApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<RedirectUrlManagementController>(
                                controller => controller.GetEnableState())
                        },
                        {
                            "tourApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<TourController>(
                                controller => controller.GetTours())
                        },
                        {
                            "embedApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<RteEmbedController>(
                                controller => controller.GetEmbed("", 0, 0))
                        },
                        {
                            "userApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<UsersController>(
                                controller => controller.PostSaveUser(null))
                        },
                        {
                            "userGroupsApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<UserGroupsController>(
                                controller => controller.PostSaveUserGroup(null))
                        },
                        {
                            "contentApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<ContentController>(
                                controller => controller.PostSave(null))
                        },
                        {
                            "publicAccessApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<PublicAccessController>(
                                controller => controller.GetPublicAccess(0))
                        },
                        {
                            "mediaApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<MediaController>(
                                controller => controller.GetRootMedia())
                        },
                        {
                            "iconApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<IconController>(
                                controller => controller.GetIcon(""))
                        },
                        {
                            "imagesApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<ImagesController>(
                                controller => controller.GetBigThumbnail(""))
                        },
                        {
                            "sectionApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<SectionController>(
                                controller => controller.GetSections())
                        },
                        {
                            "treeApplicationApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<ApplicationTreeController>(
                                controller => controller.GetApplicationTrees(null, null, null, TreeUse.None))
                        },
                        {
                            "contentTypeApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<ContentTypeController>(
                                controller => controller.GetAllowedChildren(0))
                        },
                        {
                            "mediaTypeApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<MediaTypeController>(
                                controller => controller.GetAllowedChildren(0))
                        },
                        {
                            "macroRenderingApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<MacroRenderingController>(
                                controller => controller.GetMacroParameters(0))
                        },
                        {
                            "macroApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<MacrosController>(
                                controller => controller.Create(null))
                        },
                        {
                            "authenticationApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<AuthenticationController>(
                                controller => controller.PostLogin(null))
                        },
                        {
                            "currentUserApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<CurrentUserController>(
                                controller => controller.PostChangePassword(null))
                        },
                        {
                            "entityApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<EntityController>(
                                controller => controller.GetById(0, UmbracoEntityTypes.Media))
                        },
                        {
                            "dataTypeApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<DataTypeController>(
                                controller => controller.GetById(0))
                        },
                        {
                            "dashboardApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<DashboardController>(
                                controller => controller.GetDashboard(null))
                        },
                        {
                            "logApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<LogController>(
                                controller => controller.GetPagedEntityLog(0, 0, 0, Direction.Ascending, null))
                        },
                        {
                            "memberApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<MemberController>(
                                controller => controller.GetByKey(Guid.Empty))
                        },
                        {
                            "packageInstallApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<PackageInstallController>(
                                controller => controller.Fetch(string.Empty))
                        },
                        {
                            "packageApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<PackageController>(
                                controller => controller.GetCreatedPackages())
                        },
                        {
                            "relationApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<RelationController>(
                                controller => controller.GetById(0))
                        },
                        {
                            "rteApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<RichTextPreValueController>(
                                controller => controller.GetConfiguration())
                        },
                        {
                            "stylesheetApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<StylesheetController>(
                                controller => controller.GetAll())
                        },
                        {
                            "memberTypeApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<MemberTypeController>(
                                controller => controller.GetAllTypes())
                        },
                        {
                            "memberGroupApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<MemberGroupController>(
                                controller => controller.GetAllGroups())
                        },
                        {
                            "updateCheckApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<UpdateCheckController>(
                                controller => controller.GetCheck())
                        },
                        {
                            "templateApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<TemplateController>(
                                controller => controller.GetById(0))
                        },
                        {
                            "memberTreeBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<MemberTreeController>(
                                controller => controller.GetNodes("-1", null))
                        },
                        {
                            "mediaTreeBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<MediaTreeController>(
                                controller => controller.GetNodes("-1", null))
                        },
                        {
                            "contentTreeBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<ContentTreeController>(
                                controller => controller.GetNodes("-1", null))
                        },
                        {
                            "tagsDataBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<TagsDataController>(
                                controller => controller.GetTags("", "", null))
                        },
                        {
                            "examineMgmtBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<ExamineManagementController>(
                                controller => controller.GetIndexerDetails())
                        },
                        {
                            "healthCheckBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<HealthCheckController>(
                                controller => controller.GetAllHealthChecks())
                        },
                        {
                            "templateQueryApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<TemplateQueryController>(
                                controller => controller.PostTemplateQuery(null))
                        },
                        {
                            "codeFileApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<CodeFileController>(
                                controller => controller.GetByPath("", ""))
                        },
                        {
                            "publishedStatusBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<PublishedStatusController>(
                                controller => controller.GetPublishedStatusUrl())
                        },
                        {
                            "dictionaryApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<DictionaryController>(
                                controller => controller.DeleteById(int.MaxValue))
                        },
                        {
                            "publishedSnapshotCacheStatusBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<PublishedSnapshotCacheStatusController>(
                                controller => controller.GetStatus())
                        },
                        {
                            "helpApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<HelpController>(
                                controller => controller.GetContextHelpForPage("","",""))
                        },
                        {
                            "backOfficeAssetsApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<BackOfficeAssetsController>(
                                controller => controller.GetSupportedLocales())
                        },
                        {
                            "languageApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<LanguageController>(
                                controller => controller.GetAllLanguages())
                        },
                        {
                            "relationTypeApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<RelationTypeController>(
                                controller => controller.GetById(1))
                        },
                        {
                            "logViewerApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<LogViewerController>(
                                controller => controller.GetNumberOfErrors(null, null))
                        },
                        {
                            "webProfilingBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<WebProfilingController>(
                                controller => controller.GetStatus())
                        },
                        {
                            "tinyMceApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<TinyMceController>(
                                controller => controller.UploadImage(null))
                        },
                        {
                            "imageUrlGeneratorApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<ImageUrlGeneratorController>(
                                controller => controller.GetCropUrl(null, null, null, null, null))
                        },
                        {
                            "elementTypeApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<ElementTypeController>(
                                controller => controller.GetAll())
                        },
                        {
                            "previewHubUrl", _previewRoutes.GetPreviewHubRoute()
                        },
                    }
                },
                {
                    "umbracoSettings", new Dictionary<string, object>
                    {
                        {"umbracoPath", _globalSettings.GetBackOfficePath(_hostingEnvironment)},
                        {"mediaPath", _hostingEnvironment.ToAbsolute(globalSettings.UmbracoMediaPath).TrimEnd(Constants.CharArrays.ForwardSlash)},
                        {"appPluginsPath", _hostingEnvironment.ToAbsolute(Constants.SystemDirectories.AppPlugins).TrimEnd(Constants.CharArrays.ForwardSlash)},
                        {
                            "imageFileTypes",
                            string.Join(",", _imageUrlGenerator.SupportedImageFileTypes)
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
                        {"cssPath", _hostingEnvironment.ToAbsolute(globalSettings.UmbracoCssPath).TrimEnd(Constants.CharArrays.ForwardSlash)},
                        {"allowPasswordReset", _securitySettings.AllowPasswordReset},
                        {"loginBackgroundImage", _contentSettings.LoginBackgroundImage},
                        {"loginLogoImage", _contentSettings.LoginLogoImage },
                        {"showUserInvite", _emailSender.CanSendRequiredEmail()},
                        {"canSendRequiredEmail", _emailSender.CanSendRequiredEmail()},
                        {"showAllowSegmentationForDocumentTypes", false},
                        {"minimumPasswordLength", _memberPasswordConfigurationSettings.RequiredLength},
                        {"minimumPasswordNonAlphaNum", _memberPasswordConfigurationSettings.GetMinNonAlphaNumericChars()},
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
                            // TODO: It would be nicer to not have to manually translate these properties
                            // but then needs to be changed in quite a few places in angular
                            "providers", _externalLogins.GetBackOfficeProviders()
                                .Select(p => new
                                {
                                    authType = p.AuthenticationType,
                                    caption = p.Name,
                                    properties = p.Options
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
            return Task.FromResult(defaultVals);
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
            app.Add("applicationPath", _httpContextAccessor.GetRequiredHttpContext().Request.PathBase.ToString().EnsureEndsWith('/'));

            //add the server's GMT time offset in minutes
            app.Add("serverTimeOffset", Convert.ToInt32(DateTimeOffset.Now.Offset.TotalMinutes));

            return app;
        }

        private string GetMaxRequestLength()
        {
            return _runtimeSettings.MaxRequestLength.HasValue ? _runtimeSettings.MaxRequestLength.Value.ToString() : string.Empty;
        }
    }
}
