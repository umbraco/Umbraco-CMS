using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using ClientDependency.Core.Config;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Web.Features;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.Trees;
using Umbraco.Web.WebServices;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Used to collect the server variables for use in the back office angular app
    /// </summary>
    internal class BackOfficeServerVariables
    {
        private readonly UrlHelper _urlHelper;
        private readonly ApplicationContext _applicationContext;
        private readonly HttpContextBase _httpContext;
        private readonly IOwinContext _owinContext;

        public BackOfficeServerVariables(UrlHelper urlHelper, ApplicationContext applicationContext, IUmbracoSettingsSection umbracoSettings)
        {
            _urlHelper = urlHelper;
            _applicationContext = applicationContext;
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
                {"umbracoUrls", new[] {"authenticationApiBaseUrl", "serverVarsJs", "externalLoginsUrl", "currentUserApiBaseUrl"}},
                {"umbracoSettings", new[] {"allowPasswordReset", "imageFileTypes", "maxFileSize", "loginBackgroundImage"}},
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

            //TODO: This is ultra confusing! this same key is used for different things, when returning the full app when authenticated it is this URL but when not auth'd it's actually the ServerVariables address
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
            var defaultVals = new Dictionary<string, object>
            {
                {
                    "umbracoUrls", new Dictionary<string, object>
                    {
                        //TODO: Add 'umbracoApiControllerBaseUrl' which people can use in JS
                        // to prepend their URL. We could then also use this in our own resources instead of
                        // having each url defined here explicitly - we can do that in v8! for now
                        // for umbraco services we'll stick to explicitly defining the endpoints.

                        {"externalLoginsUrl", _urlHelper.Action("ExternalLogin", "BackOffice")},
                        {"externalLinkLoginsUrl", _urlHelper.Action("LinkLogin", "BackOffice")},
                        {"legacyTreeJs", _urlHelper.Action("LegacyTreeJs", "BackOffice")},
                        {"manifestAssetList", _urlHelper.Action("GetManifestAssetList", "BackOffice")},
                        {"gridConfig", _urlHelper.Action("GetGridConfig", "BackOffice")},
                        //TODO: This is ultra confusing! this same key is used for different things, when returning the full app when authenticated it is this URL but when not auth'd it's actually the ServerVariables address
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
                                controller => controller.GetBigThumbnail(0))
                        },
                        {
                            "sectionApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<SectionController>(
                                controller => controller.GetSections())
                        },
                        {
                            "treeApplicationApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<ApplicationTreeController>(
                                controller => controller.GetApplicationTrees(null, null, null, true))
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
                            "macroApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<MacroController>(
                                controller => controller.GetMacroParameters(0))
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
                            "legacyApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<LegacyController>(
                                controller => controller.DeleteLegacyItem(null, null, null))
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
                                controller => controller.GetEntityLog(0))
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
                            "updateCheckApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<UpdateCheckController>(
                                controller => controller.GetCheck())
                        },
                        {
                            "tagApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<TagsController>(
                                controller => controller.GetAllTags(null))
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
                                controller => controller.GetTags(""))
                        },
                        {
                            "examineMgmtBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<ExamineManagementApiController>(
                                controller => controller.GetIndexerDetails())
                        },
                        {
                            "xmlDataIntegrityBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<XmlDataIntegrityController>(
                                controller => controller.CheckContentXmlTable())
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
                            "dictionaryApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<DictionaryController>(
                                controller => controller.DeleteById(int.MaxValue))
						},
                        {
                            "helpApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<HelpController>(
                                controller => controller.GetContextHelpForPage("","",""))
                        },
                        {
                            "backOfficeAssetsApiBaseUrl", _urlHelper.GetUmbracoApiServiceBaseUrl<BackOfficeAssetsController>(
                                controller => controller.GetSupportedMomentLocales())
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
                            "allowedUploadFiles",
                            string.Join(",", UmbracoConfig.For.UmbracoSettings().Content.AllowedUploadFiles)
                        },
                        {
                            "maxFileSize",
                            GetMaxRequestLength()
                        },
                        {"keepUserLoggedIn", UmbracoConfig.For.UmbracoSettings().Security.KeepUserLoggedIn},
                        {"usernameIsEmail", UmbracoConfig.For.UmbracoSettings().Security.UsernameIsEmail},
                        {"cssPath", IOHelper.ResolveUrl(SystemDirectories.Css).TrimEnd('/')},
                        {"allowPasswordReset", UmbracoConfig.For.UmbracoSettings().Security.AllowPasswordReset},
                        {"loginBackgroundImage",  UmbracoConfig.For.UmbracoSettings().Content.LoginBackgroundImage},
                        {"showUserInvite", EmailSender.CanSendRequiredEmail},
                    }
                },
                {
                    "umbracoPlugins", new Dictionary<string, object>
                    {
                        {"trees", GetTreePluginsMetaData()}
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
                            "providers", _owinContext.Authentication.GetExternalAuthenticationTypes()
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
                },
                {
                    "features", new Dictionary<string,object>
                    {
                        {
                            "disabledFeatures", new Dictionary<string,object>
                            {
                                { "disableTemplates", FeaturesResolver.Current.Features.Disabled.DisableTemplates}
                            }
                        }

                    }
                }
            };
            return defaultVals;
        }

        private IEnumerable<Dictionary<string, string>> GetTreePluginsMetaData()
        {
            var treeTypes = TreeControllerTypes.Value;
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
        /// A lazy reference to all tree controller types
        /// </summary>
        /// <remarks>
        /// We are doing this because if we constantly resolve the tree controller types from the PluginManager it will re-scan and also re-log that
        /// it's resolving which is unecessary and annoying. 
        /// </remarks>
        private static readonly Lazy<IEnumerable<Type>> TreeControllerTypes = new Lazy<IEnumerable<Type>>(() => PluginManager.Current.ResolveAttributedTreeControllers().ToArray());

        /// <summary>
        /// Returns the server variables regarding the application state
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetApplicationState()
        {
            var app = new Dictionary<string, object>
            {
                {"assemblyVersion", UmbracoVersion.AssemblyVersion}
            };

            var version = UmbracoVersion.GetSemanticVersion().ToSemanticString();

            //the value is the hash of the version, cdf version and the configured state
            app.Add("cacheBuster", $"{version}.{_applicationContext.IsConfigured}.{ClientDependencySettings.Instance.Version}".GenerateHash());

            app.Add("version", version);

            //useful for dealing with virtual paths on the client side when hosted in virtual directories especially
            app.Add("applicationPath", _httpContext.Request.ApplicationPath.EnsureEndsWith('/'));

            //add the server's GMT time offset in minutes
            app.Add("serverTimeOffset", Convert.ToInt32(DateTimeOffset.Now.Offset.TotalMinutes));

            return app;
        }

        private string GetMaxRequestLength()
        {
            var section = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
            if (section == null) return string.Empty;
            return section.MaxRequestLength.ToString();
        }
    }
}
