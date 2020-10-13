using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Media;
using Umbraco.Core.WebAssets;
using Umbraco.Web.Features;
using Umbraco.Web.Mvc;
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
        private readonly GlobalSettings _globalSettings;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly ContentSettings _contentSettings;
        private readonly TreeCollection _treeCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly RuntimeSettings _settings;
        private readonly SecuritySettings _securitySettings;
        private readonly IRuntimeMinifier _runtimeMinifier;
        private readonly IImageUrlGenerator _imageUrlGenerator;

        internal BackOfficeServerVariables(
            UrlHelper urlHelper,
            IRuntimeState runtimeState,
            UmbracoFeatures features,
            GlobalSettings globalSettings,
            IUmbracoVersion umbracoVersion,
            IOptions<ContentSettings> contentSettings,
            TreeCollection treeCollection,
            IHostingEnvironment hostingEnvironment,
            IOptions<RuntimeSettings> settings,
            IOptions<SecuritySettings> securitySettings,
            IRuntimeMinifier runtimeMinifier,
            IImageUrlGenerator imageUrlGenerator)
        {
            _urlHelper = urlHelper;
            _runtimeState = runtimeState;
            _features = features;
            _globalSettings = globalSettings;
            _umbracoVersion = umbracoVersion;
            _contentSettings = contentSettings.Value ?? throw new ArgumentNullException(nameof(contentSettings));
            _treeCollection = treeCollection ?? throw new ArgumentNullException(nameof(treeCollection));
            _hostingEnvironment = hostingEnvironment;
            _settings = settings.Value;
            _securitySettings = securitySettings.Value;
            _runtimeMinifier = runtimeMinifier;
            _imageUrlGenerator = imageUrlGenerator;
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
