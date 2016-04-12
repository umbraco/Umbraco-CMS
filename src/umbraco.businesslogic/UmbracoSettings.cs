using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Xml;
using Umbraco.Core;
using System.Collections.Generic;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using RazorDataTypeModelStaticMappingItem = umbraco.MacroEngines.RazorDataTypeModelStaticMappingItem;

namespace umbraco
{
    /// <summary>
    /// The UmbracoSettings Class contains general settings information for the entire Umbraco instance based on information from the /config/umbracoSettings.config file
    /// </summary>
    [Obsolete("Use UmbracoConfig.For.UmbracoSettings() instead, it offers all settings in strongly typed formats. This class will be removed in future versions.")]
    public class UmbracoSettings
    {
        [Obsolete("This hasn't been used since 4.1!")]
        public const string TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME = ""; // "children";

        /// <summary>
        /// Gets the umbraco settings document.
        /// </summary>
        /// <value>The _umbraco settings.</value>
        public static XmlDocument _umbracoSettings
        {
            get
            {
                var us = (XmlDocument)HttpRuntime.Cache["umbracoSettingsFile"] ?? EnsureSettingsDocument();
                return us;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the media library will create new directories in the /media directory.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if new directories are allowed otherwise, <c>false</c>.
        /// </value>
        public static bool UploadAllowDirectories
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled in umbracoSettings.config (/settings/logging/enableLogging).
        /// </summary>
        /// <value><c>true</c> if logging is enabled; otherwise, <c>false</c>.</value>
        public static bool EnableLogging
        {
            get { return UmbracoConfig.For.UmbracoSettings().Logging.EnableLogging; }
        }

        /// <summary>
        /// Gets a value indicating whether logging happens async.
        /// </summary>
        /// <value><c>true</c> if async logging is enabled; otherwise, <c>false</c>.</value>
        public static bool EnableAsyncLogging
        {
            get { return UmbracoConfig.For.UmbracoSettings().Logging.EnableAsyncLogging; }
        }

        /// <summary>
        /// Gets the assembly of an external logger that can be used to store log items in 3rd party systems
        /// </summary>
        public static string ExternalLoggerAssembly
        {
            get { return UmbracoConfig.For.UmbracoSettings().Logging.ExternalLoggerAssembly; }
        }
        /// <summary>
        /// Gets the type of an external logger that can be used to store log items in 3rd party systems
        /// </summary>
        public static string ExternalLoggerType
        {
            get { return UmbracoConfig.For.UmbracoSettings().Logging.ExternalLoggerType; }
        }

        /// <summary>
        /// Long Audit Trail to external log too
        /// </summary>
        public static bool ExternalLoggerLogAuditTrail
        {
            get { return UmbracoConfig.For.UmbracoSettings().Logging.ExternalLoggerEnableAuditTrail; }
        }

        /// <summary>
        /// Keep user alive as long as they have their browser open? Default is true
        /// </summary>
        public static bool KeepUserLoggedIn
        {
            get { return UmbracoConfig.For.UmbracoSettings().Security.KeepUserLoggedIn; }
        }
        
        /// <summary>
        /// Show disabled users in the tree in the Users section in the backoffice
        /// </summary>
        public static bool HideDisabledUsersInBackoffice
        {
            get { return UmbracoConfig.For.UmbracoSettings().Security.HideDisabledUsersInBackoffice; }
        }

        /// <summary>
        /// Enable the UI and API to allow back-office users to reset their passwords? Default is true
        /// </summary>
        public static bool AllowPasswordReset
        {
            get { return UmbracoConfig.For.UmbracoSettings().Security.AllowPasswordReset; }
        }

        /// <summary>
        /// Gets a value indicating whether the logs will be auto cleaned
        /// </summary>
        /// <value><c>true</c> if logs are to be automatically cleaned; otherwise, <c>false</c></value>
        public static bool AutoCleanLogs
        {
			get { return UmbracoConfig.For.UmbracoSettings().Logging.AutoCleanLogs; }
        }

        /// <summary>
        /// Gets the value indicating the log cleaning frequency (in miliseconds)
        /// </summary>
        public static int CleaningMiliseconds
        {
			get { return UmbracoConfig.For.UmbracoSettings().Logging.CleaningMiliseconds; }
        }

        public static int MaxLogAge
        {
            get { return UmbracoConfig.For.UmbracoSettings().Logging.MaxLogAge; }
        }

        /// <summary>
        /// Gets the disabled log types.
        /// </summary>
        /// <value>The disabled log types.</value>
        public static XmlNode DisabledLogTypes
        {
            get { return GetKeyAsNode("/settings/logging/disabledLogTypes"); }
        }

        /// <summary>
        /// Gets the package server url.
        /// </summary>
        /// <value>The package server url.</value>
        public static string PackageServer
        {
            get { return "packages.umbraco.org"; }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco will use domain prefixes.
        /// </summary>
        /// <value><c>true</c> if umbraco will use domain prefixes; otherwise, <c>false</c>.</value>
        public static bool UseDomainPrefixes
        {
            get { return UmbracoConfig.For.UmbracoSettings().RequestHandler.UseDomainPrefixes; }
        }

        /// <summary>
        /// This will add a trailing slash (/) to urls when in directory url mode
        /// NOTICE: This will always return false if Directory Urls in not active
        /// </summary>
        public static bool AddTrailingSlash
        {
			get { return UmbracoConfig.For.UmbracoSettings().RequestHandler.AddTrailingSlash; }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco will use ASP.NET MasterPages for rendering instead of its propriatary templating system.
        /// </summary>
        /// <value><c>true</c> if umbraco will use ASP.NET MasterPages; otherwise, <c>false</c>.</value>
        public static bool UseAspNetMasterPages
        {
            get { return UmbracoConfig.For.UmbracoSettings().Templates.UseAspNetMasterPages; }
        }


        /// <summary>
        /// Gets a value indicating whether umbraco will attempt to load any skins to override default template files
        /// </summary>
        /// <value><c>true</c> if umbraco will override templates with skins if present and configured <c>false</c>.</value>
        public static bool EnableTemplateFolders
        {
            get { return UmbracoConfig.For.UmbracoSettings().Templates.EnableTemplateFolders; }
        }

        /// <summary>
        /// razor DynamicNode typecasting detects XML and returns DynamicXml - Root elements that won't convert to DynamicXml
        /// </summary>
        public static List<string> NotDynamicXmlDocumentElements
        {
            get { return UmbracoConfig.For.UmbracoSettings().Scripting.NotDynamicXmlDocumentElements.Select(x => x.Element).ToList(); }
        }

        public static List<RazorDataTypeModelStaticMappingItem> RazorDataTypeModelStaticMapping
        {
			get
			{
			    var mapping = UmbracoConfig.For.UmbracoSettings().Scripting.DataTypeModelStaticMappings;
				
				//now we need to map to the old object until we can clean all this nonsense up
				return mapping.Select(x => new RazorDataTypeModelStaticMappingItem()
					{
						DataTypeGuid = x.DataTypeGuid,
						NodeTypeAlias = x.NodeTypeAlias,
						PropertyTypeAlias = x.PropertyTypeAlias,
						Raw = string.Empty,
						TypeName = x.MappingName
					}).ToList();
			}
        }

        /// <summary>
        /// Gets a value indicating whether umbraco will clone XML cache on publish.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if umbraco will clone XML cache on publish; otherwise, <c>false</c>.
        /// </value>
        public static bool CloneXmlCacheOnPublish
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.CloneXmlContent; }
        }

        /// <summary>
        /// Gets a value indicating whether rich text editor content should be parsed by tidy.
        /// </summary>
        /// <value><c>true</c> if content is parsed; otherwise, <c>false</c>.</value>
        public static bool TidyEditorContent
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.TidyEditorContent; }
        }

        /// <summary>
        /// Gets the encoding type for the tidyied content.
        /// </summary>
        /// <value>The encoding type as string.</value>
        public static string TidyCharEncoding
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.TidyCharEncoding; }
        }

        /// <summary>
        /// Gets the property context help option, this can either be 'text', 'icon' or 'none'
        /// </summary>
        /// <value>The property context help option.</value>
        public static string PropertyContextHelpOption
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.PropertyContextHelpOption; }
        }

        public static string DefaultBackofficeProvider
        {
            get { return UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider; }
        }

        /// <summary>
        /// Whether to force safe aliases (no spaces, no special characters) at businesslogic level on contenttypes and propertytypes
        /// </summary>
        public static bool ForceSafeAliases
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.ForceSafeAliases; }
        }

        /// <summary>
        /// File types that will not be allowed to be uploaded via the content/media upload control
        /// </summary>
        public static IEnumerable<string> DisallowedUploadFiles
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.DisallowedUploadFiles; }
        }

        /// <summary>
        /// Gets the allowed image file types.
        /// </summary>
        /// <value>The allowed image file types.</value>
        public static string ImageFileTypes
        {
            get { return string.Join(",", UmbracoConfig.For.UmbracoSettings().Content.ImageFileTypes.Select(x => x.ToLowerInvariant())); }
        }

        /// <summary>
        /// Gets the allowed script file types.
        /// </summary>
        /// <value>The allowed script file types.</value>
        public static string ScriptFileTypes
        {
            get { return string.Join(",", UmbracoConfig.For.UmbracoSettings().Content.ScriptFileTypes); }
        }

        /// <summary>
        /// Gets the duration in seconds to cache queries to umbraco library member and media methods
        /// Default is 1800 seconds (30 minutes)
        /// </summary>
        public static int UmbracoLibraryCacheDuration
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.UmbracoLibraryCacheDuration; }
        }

        /// <summary>
        /// Gets the path to the scripts folder used by the script editor.
        /// </summary>
        /// <value>The script folder path.</value>
        public static string ScriptFolderPath
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.ScriptFolderPath; }
        }

        /// <summary>
        /// Enabled or disable the script/code editor
        /// </summary>
        public static bool ScriptDisableEditor
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.ScriptEditorDisable; }
        }
        
        /// <summary>
        /// Gets a value indicating whether umbraco will ensure unique node naming.
        /// This will ensure that nodes cannot have the same url, but will add extra characters to a url.
        /// ex: existingnodename.aspx would become existingnodename(1).aspx if a node with the same name is found 
        /// </summary>
        /// <value><c>true</c> if umbraco ensures unique node naming; otherwise, <c>false</c>.</value>
        public static bool EnsureUniqueNaming
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.EnsureUniqueNaming; }
        }

        /// <summary>
        /// Gets the notification email sender.
        /// </summary>
        /// <value>The notification email sender.</value>
        public static string NotificationEmailSender
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.NotificationEmailAddress; }
        }

        /// <summary>
        /// Gets a value indicating whether notification-emails are HTML.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if html notification-emails are disabled; otherwise, <c>false</c>.
        /// </value>
        public static bool NotificationDisableHtmlEmail
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.DisableHtmlEmail; }
        }

        /// <summary>
        /// Gets the allowed attributes on images.
        /// </summary>
        /// <value>The allowed attributes on images.</value>
        public static string ImageAllowedAttributes
        {
            get { return string.Join(",", UmbracoConfig.For.UmbracoSettings().Content.ImageTagAllowedAttributes); }
        }

        public static XmlNode ImageAutoFillImageProperties
        {
            get { return GetKeyAsNode("/settings/content/imaging/autoFillImageProperties"); }
        }

        /// <summary>
        /// Gets the scheduled tasks as XML
        /// </summary>
        /// <value>The scheduled tasks.</value>
        public static XmlNode ScheduledTasks
        {
            get { return GetKeyAsNode("/settings/scheduledTasks"); }
        }

        /// <summary>
        /// Gets a list of characters that will be replaced when generating urls
        /// </summary>
        /// <value>The URL replacement characters.</value>
        public static XmlNode UrlReplaceCharacters
        {
            get { return GetKeyAsNode("/settings/requestHandler/urlReplacing"); }
        }

        /// <summary>
        /// Whether to replace double dashes from url (ie my--story----from--dash.aspx caused by multiple url replacement chars
        /// </summary>
        public static bool RemoveDoubleDashesFromUrlReplacing
        {
            get { return UmbracoConfig.For.UmbracoSettings().RequestHandler.RemoveDoubleDashes; }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco will use distributed calls.
        /// This enables umbraco to share cache and content across multiple servers.
        /// Used for load-balancing high-traffic sites.
        /// </summary>
        /// <value><c>true</c> if umbraco uses distributed calls; otherwise, <c>false</c>.</value>
        public static bool UseDistributedCalls
        {
            get { return UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled; }
        }


        /// <summary>
        /// Gets the ID of the user with access rights to perform the distributed calls.
        /// </summary>
        /// <value>The distributed call user.</value>
        public static int DistributedCallUser
        {
            get { return UmbracoConfig.For.UmbracoSettings().DistributedCall.UserId; }
        }

        /// <summary>
        /// Gets the html injected into a (x)html page if Umbraco is running in preview mode
        /// </summary>
        public static string PreviewBadge
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.PreviewBadge; }
        }

        /// <summary>
        /// Gets IP or hostnames of the distribution servers.
        /// These servers will receive a call everytime content is created/deleted/removed
        /// and update their content cache accordingly, ensuring a consistent cache on all servers
        /// </summary>
        /// <value>The distribution servers.</value>
        public static XmlNode DistributionServers
        {
            get
            {
                try
                {
                    return GetKeyAsNode("/settings/distributedCall/servers");
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets HelpPage configurations.
        /// A help page configuration specify language, user type, application, application url and 
        /// the target help page url.
        /// </summary>
        public static XmlNode HelpPages
        {
            get
            {
                try
                {
                    return GetKeyAsNode("/settings/help");
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets all repositories registered, and returns them as XmlNodes, containing name, alias and webservice url.
        /// These repositories are used by the built-in package installer and uninstaller to install new packages and check for updates.
        /// All repositories should have a unique alias.
        /// All packages installed from a repository gets the repository alias included in the install information
        /// </summary>
        /// <value>The repository servers.</value>
        public static XmlNode Repositories
        {
            get
            {
                try
                {
                    return GetKeyAsNode("/settings/repositories");
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco will use the viewstate mover module.
        /// The viewstate mover will move all asp.net viewstate information to the bottom of the aspx page
        /// to ensure that search engines will index text instead of javascript viewstate information.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if umbraco will use the viewstate mover module; otherwise, <c>false</c>.
        /// </value>
        public static bool UseViewstateMoverModule
        {
            get { return UmbracoConfig.For.UmbracoSettings().ViewStateMoverModule.Enable; }
        }


        /// <summary>
        /// Tells us whether the Xml Content cache is disabled or not
        /// Default is enabled
        /// </summary>
        public static bool isXmlContentCacheDisabled
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.XmlCacheEnabled == false; }
        }

        /// <summary>
        /// Check if there's changes to the umbraco.config xml file cache on disk on each request
        /// Makes it possible to updates environments by syncing the umbraco.config file across instances
        /// Relates to http://umbraco.codeplex.com/workitem/30722
        /// </summary>
        public static bool XmlContentCheckForDiskChanges
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.XmlContentCheckForDiskChanges; }
        }

        /// <summary>
        /// If this is enabled, all Umbraco objects will generate data in the preview table (cmsPreviewXml).
        /// If disabled, only documents will generate data.
        /// This feature is useful if anyone would like to see how data looked at a given time
        /// </summary>
        public static bool EnableGlobalPreviewStorage
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled; }
        }

        /// <summary>
        /// Whether to use the new 4.1 schema or the old legacy schema
        /// </summary>
        /// <value>
        /// 	<c>true</c> if yes, use the old node/data model; otherwise, <c>false</c>.
        /// </value>
        public static bool UseLegacyXmlSchema
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema; }
        }

    	public static IEnumerable<string> AppCodeFileExtensionsList
    	{
            get { return UmbracoConfig.For.UmbracoSettings().Developer.AppCodeFileExtensions.Select(x => x.Extension); }
    	}

		[Obsolete("Use AppCodeFileExtensionsList instead")]
        public static XmlNode AppCodeFileExtensions
        {
            get
            {
                XmlNode value = GetKeyAsNode("/settings/developer/appCodeFileExtensions");
                if (value != null)
                {
                    return value;
                }

                // default is .cs and .vb
                value = _umbracoSettings.CreateElement("appCodeFileExtensions");
                value.AppendChild(XmlHelper.AddTextNode(_umbracoSettings, "ext", "cs"));
                value.AppendChild(XmlHelper.AddTextNode(_umbracoSettings, "ext", "vb"));
                return value;
            }
        }

        /// <summary>
        /// Tells us whether the Xml to always update disk cache, when changes are made to content
        /// Default is enabled
        /// </summary>
        public static bool continouslyUpdateXmlDiskCache
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.ContinouslyUpdateXmlDiskCache; }
        }

        /// <summary>
        /// Tells us whether to use a splash page while umbraco is initializing content. 
        /// If not, requests are queued while umbraco loads content. For very large sites (+10k nodes) it might be usefull to 
        /// have a splash page
        /// Default is disabled
        /// </summary>
        public static bool EnableSplashWhileLoading
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.EnableSplashWhileLoading; }
        }

        public static bool ResolveUrlsFromTextString
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.ResolveUrlsFromTextString; }
        }

        /// <summary>
        /// This configuration setting defines how to handle macro errors:
        /// - Inline - Show error within macro as text (default and current Umbraco 'normal' behavior)
        /// - Silent - Suppress error and hide macro
        /// - Throw  - Throw an exception and invoke the global error handler (if one is defined, if not you'll get a YSOD)
        /// </summary>
        /// <value>MacroErrorBehaviour enum defining how to handle macro errors.</value>
        public static MacroErrorBehaviour MacroErrorBehaviour
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.MacroErrorBehaviour; }
        }

        /// <summary>
        /// This configuration setting defines how to show icons in the document type editor. 
        /// - ShowDuplicates       - Show duplicates in files and sprites. (default and current Umbraco 'normal' behaviour)
        /// - HideSpriteDuplicates - Show files on disk and hide duplicates from the sprite
        /// - HideFileDuplicates   - Show files in the sprite and hide duplicates on disk
        /// </summary>
        /// <value>MacroErrorBehaviour enum defining how to show icons in the document type editor.</value>
        [Obsolete("This is no longer used and will be removed from the core in future versions")]
        public static IconPickerBehaviour IconPickerBehaviour
        {
            get { return IconPickerBehaviour.ShowDuplicates; }
        }

        /// <summary>
        /// Gets the default document type property used when adding new properties through the back-office
        /// </summary>
        /// <value>Configured text for the default document type property</value>
        /// <remarks>If undefined, 'Textstring' is the default</remarks>
        public static string DefaultDocumentTypeProperty
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.DefaultDocumentTypeProperty; }
        }

        /// <summary>
        /// Enables inherited document types.
        /// This feature is not recommended and therefore is not enabled by default in new installations.
        /// Inherited document types will not be supported in v8.
        /// </summary>
        //[Obsolete("This will not be supported in v8")]
        public static bool EnableInheritedDocumentTypes
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.EnableInheritedDocumentTypes; }
        }

        private static string _path;

        /// <summary>
        /// Gets the settings file path
        /// </summary>
        internal static string SettingsFilePath
        {
            get { return _path ?? (_path = GlobalSettings.FullpathToRoot + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar); }
        }

        internal const string Filename = "umbracoSettings.config";

        internal static XmlDocument EnsureSettingsDocument()
        {
            var settingsFile = HttpRuntime.Cache["umbracoSettingsFile"];

            // Check for language file in cache
            if (settingsFile == null)
            {
                var temp = new XmlDocument();
                var settingsReader = new XmlTextReader(SettingsFilePath + Filename);
                try
                {
                    temp.Load(settingsReader);
                    HttpRuntime.Cache.Insert("umbracoSettingsFile", temp,
                                             new CacheDependency(SettingsFilePath + Filename));
                }
                catch (XmlException e)
                {
                    throw new XmlException("Your umbracoSettings.config file fails to pass as valid XML. Refer to the InnerException for more information", e);
                }
                catch (Exception e)
                {
                    LogHelper.Error<UmbracoSettings>("Error reading umbracoSettings file: " + e.ToString(), e);
                }
                settingsReader.Close();
                return temp;
            }
            else
                return (XmlDocument)settingsFile;
        }

        internal static void Save()
        {
            _umbracoSettings.Save(SettingsFilePath + Filename);
        }


        /// <summary>
        /// Selects a xml node in the umbraco settings config file.
        /// </summary>
        /// <param name="key">The xpath query to the specific node.</param>
        /// <returns>If found, it returns the specific configuration xml node.</returns>
        internal static XmlNode GetKeyAsNode(string key)
        {
            if (key == null)
                throw new ArgumentException("Key cannot be null");
            EnsureSettingsDocument();
            if (_umbracoSettings == null || _umbracoSettings.DocumentElement == null)
                return null;
            return _umbracoSettings.DocumentElement.SelectSingleNode(key);
        }

        /// <summary>
        /// Gets the value of configuration xml node with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        internal static string GetKey(string key)
        {
            EnsureSettingsDocument();

            string attrName = null;
            var pos = key.IndexOf('@');
            if (pos > 0)
            {
                attrName = key.Substring(pos + 1);
                key = key.Substring(0, pos - 1);
            }

            var node = _umbracoSettings.DocumentElement.SelectSingleNode(key);
            if (node == null)
                return string.Empty;

            if (pos < 0)
            {
                if (node.FirstChild == null || node.FirstChild.Value == null)
                    return string.Empty;
                return node.FirstChild.Value;
            }
            else
            {
                var attr = node.Attributes[attrName];
                if (attr == null)
                    return string.Empty;
                return attr.Value;
            }
        }

    }
}