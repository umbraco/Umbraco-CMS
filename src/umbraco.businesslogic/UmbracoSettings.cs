using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Xml;
using umbraco.BusinessLogic;
using System.Collections.Generic;
using umbraco.MacroEngines;

namespace umbraco
{
    /// <summary>
    /// The UmbracoSettings Class contains general settings information for the entire Umbraco instance based on information from the /config/umbracoSettings.config file
    /// </summary>
    public class UmbracoSettings
    {
        public const string TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME = ""; // "children";

        /// <summary>
        /// Gets the umbraco settings document.
        /// </summary>
        /// <value>The _umbraco settings.</value>
        public static XmlDocument _umbracoSettings
        {
            get { return Umbraco.Core.Configuration.UmbracoSettings.UmbracoSettingsXmlDoc; }
        }

		/// <summary>
		/// Gets/sets the settings file path, the setter can be used in unit tests
		/// </summary>
		internal static string SettingsFilePath
    	{
    		get { return Umbraco.Core.Configuration.UmbracoSettings.SettingsFilePath; }
			set { Umbraco.Core.Configuration.UmbracoSettings.SettingsFilePath = value; }
    	}

        /// <summary>
        /// Selects a xml node in the umbraco settings config file.
        /// </summary>
        /// <param name="Key">The xpath query to the specific node.</param>
        /// <returns>If found, it returns the specific configuration xml node.</returns>
        public static XmlNode GetKeyAsNode(string Key)
        {
			return Umbraco.Core.Configuration.UmbracoSettings.GetKeyAsNode(Key);
        }

        /// <summary>
        /// Gets the value of configuration xml node with the specified key.
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <returns></returns>
        public static string GetKey(string Key)
        {
			return Umbraco.Core.Configuration.UmbracoSettings.GetKey(Key);
        }

        /// <summary>
        /// Gets a value indicating whether the media library will create new directories in the /media directory.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if new directories are allowed otherwise, <c>false</c>.
        /// </value>
        public static bool UploadAllowDirectories
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.UploadAllowDirectories; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled in umbracoSettings.config (/settings/logging/enableLogging).
        /// </summary>
        /// <value><c>true</c> if logging is enabled; otherwise, <c>false</c>.</value>
        public static bool EnableLogging
        {
            get { return Umbraco.Core.Configuration.UmbracoSettings.EnableLogging; }
        }

        /// <summary>
        /// Gets a value indicating whether logging happens async.
        /// </summary>
        /// <value><c>true</c> if async logging is enabled; otherwise, <c>false</c>.</value>
        public static bool EnableAsyncLogging
        {
            get { return Umbraco.Core.Configuration.UmbracoSettings.EnableAsyncLogging; }
        }

        /// <summary>
        /// Gets the assembly of an external logger that can be used to store log items in 3rd party systems
        /// </summary>
        public static string ExternalLoggerAssembly
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.ExternalLoggerAssembly; }
        }
        /// <summary>
        /// Gets the type of an external logger that can be used to store log items in 3rd party systems
        /// </summary>
        public static string ExternalLoggerType
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.ExternalLoggerType; }
        }

        /// <summary>
        /// Long Audit Trail to external log too
        /// </summary>
        public static bool ExternalLoggerLogAuditTrail
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.ExternalLoggerLogAuditTrail; }
        }

        /// <summary>
        /// Keep user alive as long as they have their browser open? Default is true
        /// </summary>
        public static bool KeepUserLoggedIn
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.KeepUserLoggedIn; }
        }

        /// <summary>
        /// Show disabled users in the tree in the Users section in the backoffice
        /// </summary>
        public static bool HideDisabledUsersInBackoffice
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.HideDisabledUsersInBackoffice; }
        }

        /// <summary>
        /// Gets a value indicating whether the logs will be auto cleaned
        /// </summary>
        /// <value><c>true</c> if logs are to be automatically cleaned; otherwise, <c>false</c></value>
        public static bool AutoCleanLogs
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.AutoCleanLogs; }
        }

        /// <summary>
        /// Gets the value indicating the log cleaning frequency (in miliseconds)
        /// </summary>
        public static int CleaningMiliseconds
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.CleaningMiliseconds; }
        }

        public static int MaxLogAge
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.MaxLogAge; }
        }

        /// <summary>
        /// Gets the disabled log types.
        /// </summary>
        /// <value>The disabled log types.</value>
        public static XmlNode DisabledLogTypes
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.DisabledLogTypes; }
        }

        /// <summary>
        /// Gets the package server url.
        /// </summary>
        /// <value>The package server url.</value>
        public static string PackageServer
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.PackageServer; }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco will use domain prefixes.
        /// </summary>
        /// <value><c>true</c> if umbraco will use domain prefixes; otherwise, <c>false</c>.</value>
        public static bool UseDomainPrefixes
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.UseDomainPrefixes; }
        }

        /// <summary>
        /// This will add a trailing slash (/) to urls when in directory url mode
        /// NOTICE: This will always return false if Directory Urls in not active
        /// </summary>
        public static bool AddTrailingSlash
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.AddTrailingSlash; }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco will use ASP.NET MasterPages for rendering instead of its propriatary templating system.
        /// </summary>
        /// <value><c>true</c> if umbraco will use ASP.NET MasterPages; otherwise, <c>false</c>.</value>
        public static bool UseAspNetMasterPages
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.UseAspNetMasterPages; }
        }


        /// <summary>
        /// Gets a value indicating whether umbraco will attempt to load any skins to override default template files
        /// </summary>
        /// <value><c>true</c> if umbraco will override templates with skins if present and configured <c>false</c>.</value>
        public static bool EnableTemplateFolders
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.EnableTemplateFolders; }
        }

        /// <summary>
        /// razor DynamicNode typecasting detects XML and returns DynamicXml - Root elements that won't convert to DynamicXml
        /// </summary>
        public static List<string> NotDynamicXmlDocumentElements
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.NotDynamicXmlDocumentElements.ToList(); }
        }

        public static List<RazorDataTypeModelStaticMappingItem> RazorDataTypeModelStaticMapping
        {
			get
			{
				var mapping = Umbraco.Core.Configuration.UmbracoSettings.RazorDataTypeModelStaticMapping;
				
				//now we need to map to the old object until we can clean all this nonsense up
				return mapping.Select(x => new RazorDataTypeModelStaticMappingItem()
					{
						DataTypeGuid = x.DataTypeGuid,
						NodeTypeAlias = x.NodeTypeAlias,
						PropertyTypeAlias = x.PropertyTypeAlias,
						Raw = x.Raw,
						TypeName = x.TypeName
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
			get { return Umbraco.Core.Configuration.UmbracoSettings.CloneXmlCacheOnPublish; }
        }

        /// <summary>
        /// Gets a value indicating whether rich text editor content should be parsed by tidy.
        /// </summary>
        /// <value><c>true</c> if content is parsed; otherwise, <c>false</c>.</value>
        public static bool TidyEditorContent
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.TidyEditorContent; }
        }

        /// <summary>
        /// Gets the encoding type for the tidyied content.
        /// </summary>
        /// <value>The encoding type as string.</value>
        public static string TidyCharEncoding
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.TidyCharEncoding; }
        }

        /// <summary>
        /// Gets the property context help option, this can either be 'text', 'icon' or 'none'
        /// </summary>
        /// <value>The property context help option.</value>
        public static string PropertyContextHelpOption
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.PropertyContextHelpOption; }
        }

        public static string DefaultBackofficeProvider
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.DefaultBackofficeProvider; }
        }

        /// <summary>
        /// Whether to force safe aliases (no spaces, no special characters) at businesslogic level on contenttypes and propertytypes
        /// </summary>
        public static bool ForceSafeAliases
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.ForceSafeAliases; }
        }


        /// <summary>
        /// Gets the allowed image file types.
        /// </summary>
        /// <value>The allowed image file types.</value>
        public static string ImageFileTypes
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.ImageFileTypes; }
        }

        /// <summary>
        /// Gets the allowed script file types.
        /// </summary>
        /// <value>The allowed script file types.</value>
        public static string ScriptFileTypes
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.ScriptFileTypes; }
        }

        /// <summary>
        /// Gets the duration in seconds to cache queries to umbraco library member and media methods
        /// Default is 1800 seconds (30 minutes)
        /// </summary>
        public static int UmbracoLibraryCacheDuration
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.UmbracoLibraryCacheDuration; }
        }

        /// <summary>
        /// Gets the path to the scripts folder used by the script editor.
        /// </summary>
        /// <value>The script folder path.</value>
        public static string ScriptFolderPath
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.ScriptFolderPath; }
        }

        /// <summary>
        /// Enabled or disable the script/code editor
        /// </summary>
        public static bool ScriptDisableEditor
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.ScriptDisableEditor; }
        }
        
        /// <summary>
        /// Gets a value indicating whether umbraco will ensure unique node naming.
        /// This will ensure that nodes cannot have the same url, but will add extra characters to a url.
        /// ex: existingnodename.aspx would become existingnodename(1).aspx if a node with the same name is found 
        /// </summary>
        /// <value><c>true</c> if umbraco ensures unique node naming; otherwise, <c>false</c>.</value>
        public static bool EnsureUniqueNaming
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.EnsureUniqueNaming; }
        }

        /// <summary>
        /// Gets the notification email sender.
        /// </summary>
        /// <value>The notification email sender.</value>
        public static string NotificationEmailSender
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.NotificationEmailSender; }
        }

        /// <summary>
        /// Gets a value indicating whether notification-emails are HTML.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if html notification-emails are disabled; otherwise, <c>false</c>.
        /// </value>
        public static bool NotificationDisableHtmlEmail
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.NotificationDisableHtmlEmail; }
        }

        /// <summary>
        /// Gets the allowed attributes on images.
        /// </summary>
        /// <value>The allowed attributes on images.</value>
        public static string ImageAllowedAttributes
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.ImageAllowedAttributes; }
        }

        public static XmlNode ImageAutoFillImageProperties
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.ImageAutoFillImageProperties; }
        }

        /// <summary>
        /// Gets the scheduled tasks as XML
        /// </summary>
        /// <value>The scheduled tasks.</value>
        public static XmlNode ScheduledTasks
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.ScheduledTasks; }
        }

        /// <summary>
        /// Gets a list of characters that will be replaced when generating urls
        /// </summary>
        /// <value>The URL replacement characters.</value>
        public static XmlNode UrlReplaceCharacters
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.UrlReplaceCharacters; }
        }

        /// <summary>
        /// Whether to replace double dashes from url (ie my--story----from--dash.aspx caused by multiple url replacement chars
        /// </summary>
        public static bool RemoveDoubleDashesFromUrlReplacing
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.RemoveDoubleDashesFromUrlReplacing; }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco will use distributed calls.
        /// This enables umbraco to share cache and content across multiple servers.
        /// Used for load-balancing high-traffic sites.
        /// </summary>
        /// <value><c>true</c> if umbraco uses distributed calls; otherwise, <c>false</c>.</value>
        public static bool UseDistributedCalls
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.UseDistributedCalls; }
        }


        /// <summary>
        /// Gets the ID of the user with access rights to perform the distributed calls.
        /// </summary>
        /// <value>The distributed call user.</value>
        public static int DistributedCallUser
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.DistributedCallUser; }
        }

        /// <summary>
        /// Gets the html injected into a (x)html page if Umbraco is running in preview mode
        /// </summary>
        public static string PreviewBadge
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.PreviewBadge; }
        }

        /// <summary>
        /// Gets IP or hostnames of the distribution servers.
        /// These servers will receive a call everytime content is created/deleted/removed
        /// and update their content cache accordingly, ensuring a consistent cache on all servers
        /// </summary>
        /// <value>The distribution servers.</value>
        public static XmlNode DistributionServers
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.DistributionServers; }
        }

        /// <summary>
        /// Gets HelpPage configurations.
        /// A help page configuration specify language, user type, application, application url and 
        /// the target help page url.
        /// </summary>
        public static XmlNode HelpPages
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.HelpPages; }
        }

        /// <summary>
        /// Gets all repositories registered, and returns them as XmlNodes, containing name, alias and webservice url.
        /// These repositories are used by the build-in package installer and uninstaller to install new packages and check for updates.
        /// All repositories should have a unique alias.
        /// All packages installed from a repository gets the repository alias included in the install information
        /// </summary>
        /// <value>The repository servers.</value>
        public static XmlNode Repositories
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.Repositories; }
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
			get { return Umbraco.Core.Configuration.UmbracoSettings.UseViewstateMoverModule; }
        }


        /// <summary>
        /// Tells us whether the Xml Content cache is disabled or not
        /// Default is enabled
        /// </summary>
        public static bool isXmlContentCacheDisabled
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.IsXmlContentCacheDisabled; }
        }

        /// <summary>
        /// Check if there's changes to the umbraco.config xml file cache on disk on each request
        /// Makes it possible to updates environments by syncing the umbraco.config file across instances
        /// Relates to http://umbraco.codeplex.com/workitem/30722
        /// </summary>
        public static bool XmlContentCheckForDiskChanges
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.XmlContentCheckForDiskChanges; }
        }

        /// <summary>
        /// If this is enabled, all Umbraco objects will generate data in the preview table (cmsPreviewXml).
        /// If disabled, only documents will generate data.
        /// This feature is useful if anyone would like to see how data looked at a given time
        /// </summary>
        public static bool EnableGlobalPreviewStorage
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.EnableGlobalPreviewStorage; }
        }

        /// <summary>
        /// Whether to use the new 4.1 schema or the old legacy schema
        /// </summary>
        /// <value>
        /// 	<c>true</c> if yes, use the old node/data model; otherwise, <c>false</c>.
        /// </value>
        public static bool UseLegacyXmlSchema
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.UseLegacyXmlSchema; }
        }

    	public static IEnumerable<string> AppCodeFileExtensionsList
    	{
			get { return Umbraco.Core.Configuration.UmbracoSettings.AppCodeFileExtensionsList; }
    	}

		[Obsolete("Use AppCodeFileExtensionsList instead")]
        public static XmlNode AppCodeFileExtensions
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.AppCodeFileExtensions; }
        }

        /// <summary>
        /// Tells us whether the Xml to always update disk cache, when changes are made to content
        /// Default is enabled
        /// </summary>
        public static bool continouslyUpdateXmlDiskCache
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.ContinouslyUpdateXmlDiskCache; }
        }

        /// <summary>
        /// Tells us whether to use a splash page while umbraco is initializing content. 
        /// If not, requests are queued while umbraco loads content. For very large sites (+10k nodes) it might be usefull to 
        /// have a splash page
        /// Default is disabled
        /// </summary>
        public static bool EnableSplashWhileLoading
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.EnableSplashWhileLoading; }
        }

        public static bool ResolveUrlsFromTextString
        {
			get { return Umbraco.Core.Configuration.UmbracoSettings.ResolveUrlsFromTextString; }
        }


        /// <summary>
        /// Enables MVC, and at the same time disable webform masterpage templates.
        /// This ensure views are automaticly created instead of masterpages.
        /// Views are display in the tree instead of masterpages and a MVC template editor
        /// is used instead of the masterpages editor
        /// </summary>
        /// <value><c>true</c> if umbraco defaults to using MVC views for templating, otherwise <c>false</c>.</value>
        public static bool EnableMvcSupport
        {
            get { return Umbraco.Core.Configuration.UmbracoSettings.EnableMvcSupport; }
        }

        /// <summary>
        /// Configuration regarding webservices
        /// </summary>
        /// <remarks>Put in seperate class for more logik/seperation</remarks>
        public class Webservices
        {
            /// <summary>
            /// Gets a value indicating whether this <see cref="Webservices"/> is enabled.
            /// </summary>
            /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
            public static bool Enabled
            {
				get { return Umbraco.Core.Configuration.UmbracoSettings.WebServices.Enabled; }
            }

            #region "Webservice configuration"

            /// <summary>
            /// Gets the document service users who have access to use the document web service
            /// </summary>
            /// <value>The document service users.</value>
            public static string[] documentServiceUsers
            {
				get { return Umbraco.Core.Configuration.UmbracoSettings.WebServices.DocumentServiceUsers; }
            }

            /// <summary>
            /// Gets the file service users who have access to use the file web service
            /// </summary>
            /// <value>The file service users.</value>
            public static string[] fileServiceUsers
            {
				get { return Umbraco.Core.Configuration.UmbracoSettings.WebServices.FileServiceUsers; }
            }


            /// <summary>
            /// Gets the folders used by the file web service
            /// </summary>
            /// <value>The file service folders.</value>
            public static string[] fileServiceFolders
            {
				get { return Umbraco.Core.Configuration.UmbracoSettings.WebServices.FileServiceFolders; }
            }

            /// <summary>
            /// Gets the member service users who have access to use the member web service
            /// </summary>
            /// <value>The member service users.</value>
            public static string[] memberServiceUsers
            {
				get { return Umbraco.Core.Configuration.UmbracoSettings.WebServices.MemberServiceUsers; }
            }

            /// <summary>
            /// Gets the stylesheet service users who have access to use the stylesheet web service
            /// </summary>
            /// <value>The stylesheet service users.</value>
            public static string[] stylesheetServiceUsers
            {
				get { return Umbraco.Core.Configuration.UmbracoSettings.WebServices.StylesheetServiceUsers; }
            }

            /// <summary>
            /// Gets the template service users who have access to use the template web service
            /// </summary>
            /// <value>The template service users.</value>
            public static string[] templateServiceUsers
            {
				get { return Umbraco.Core.Configuration.UmbracoSettings.WebServices.TemplateServiceUsers; }
            }

            /// <summary>
            /// Gets the media service users who have access to use the media web service
            /// </summary>
            /// <value>The media service users.</value>
            public static string[] mediaServiceUsers
            {
				get { return Umbraco.Core.Configuration.UmbracoSettings.WebServices.MediaServiceUsers; }
            }


            /// <summary>
            /// Gets the maintenance service users who have access to use the maintance web service
            /// </summary>
            /// <value>The maintenance service users.</value>
            public static string[] maintenanceServiceUsers
            {
				get { return Umbraco.Core.Configuration.UmbracoSettings.WebServices.MaintenanceServiceUsers; }
            }

            #endregion
        }
    }
}