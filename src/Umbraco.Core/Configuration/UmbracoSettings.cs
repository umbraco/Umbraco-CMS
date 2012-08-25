using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Xml;

using System.Collections.Generic;
using Umbraco.Core.Logging;


namespace Umbraco.Core.Configuration
{
	//NOTE: Do not expose this class ever until we cleanup all configuration including removal of static classes, etc...
	// we have this two tasks logged:
	// http://issues.umbraco.org/issue/U4-58
	// http://issues.umbraco.org/issue/U4-115

	//TODO: Re-enable logging !!!!

    /// <summary>
    /// The UmbracoSettings Class contains general settings information for the entire Umbraco instance based on information from the /config/umbracoSettings.config file
    /// </summary>
    internal class UmbracoSettings
    {
        internal const string TempFriendlyXmlChildContainerNodename = ""; // "children";

        /// <summary>
        /// Gets the umbraco settings document.
        /// </summary>
        /// <value>The _umbraco settings.</value>
		internal static XmlDocument UmbracoSettingsXmlDoc
        {
            get
            {
                var us = (XmlDocument)HttpRuntime.Cache["umbracoSettingsFile"] ?? EnsureSettingsDocument();
            	return us;
            }
        }

    	private static string _path;

		/// <summary>
		/// Gets/sets the settings file path, the setter can be used in unit tests
		/// </summary>
		internal static string SettingsFilePath
    	{
    		get { return _path ?? (_path = GlobalSettings.FullpathToRoot + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar); }
			set { _path = value; }
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
            UmbracoSettingsXmlDoc.Save(SettingsFilePath + Filename);
        }


        /// <summary>
        /// Selects a xml node in the umbraco settings config file.
        /// </summary>
        /// <param name="key">The xpath query to the specific node.</param>
        /// <returns>If found, it returns the specific configuration xml node.</returns>
        public static XmlNode GetKeyAsNode(string key)
        {
            if (key == null)
                throw new ArgumentException("Key cannot be null");
            EnsureSettingsDocument();
            if (UmbracoSettingsXmlDoc == null || UmbracoSettingsXmlDoc.DocumentElement == null)
                return null;
            return UmbracoSettingsXmlDoc.DocumentElement.SelectSingleNode(key);
        }

        /// <summary>
        /// Gets the value of configuration xml node with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetKey(string key)
        {
            EnsureSettingsDocument();

            var node = UmbracoSettingsXmlDoc.DocumentElement.SelectSingleNode(key);
            if (node == null || node.FirstChild == null || node.FirstChild.Value == null)
                return string.Empty;
            return node.FirstChild.Value;
        }

        /// <summary>
        /// Gets a value indicating whether the media library will create new directories in the /media directory.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if new directories are allowed otherwise, <c>false</c>.
        /// </value>
        public static bool UploadAllowDirectories
        {
            get { return bool.Parse(GetKey("/settings/content/UploadAllowDirectories")); }
        }

		/// <summary>
		/// THIS IS TEMPORARY until we fix up settings all together, this setting is actually not 'settable' but is
		/// here for future purposes since we check for thsi settings in the module.
		/// </summary>
    	internal static bool EnableBaseRestHandler
    	{
    		get { return true; }
    	}

		/// <summary>
		/// THIS IS TEMPORARY until we fix up settings all together, this setting is actually not 'settable' but is
		/// here for future purposes since we check for thsi settings in the module.
		/// </summary>
		internal static string BootSplashPage
		{
			get { return "~/default.aspx"; }
		}

        /// <summary>
        /// Gets a value indicating whether logging is enabled in umbracoSettings.config (/settings/logging/enableLogging).
        /// </summary>
        /// <value><c>true</c> if logging is enabled; otherwise, <c>false</c>.</value>
		public static bool EnableLogging
        {
            get
            {
            	// We return true if no enable logging element is present in 
                // umbracoSettings (to enable default behaviour when upgrading)
                var enableLogging = GetKey("/settings/logging/enableLogging");
            	return String.IsNullOrEmpty(enableLogging) || bool.Parse(enableLogging);
            }
        }

        /// <summary>
        /// Gets a value indicating whether logging happens async.
        /// </summary>
        /// <value><c>true</c> if async logging is enabled; otherwise, <c>false</c>.</value>
        public static bool EnableAsyncLogging
        {
            get
            {
                string value = GetKey("/settings/logging/enableAsyncLogging");
                bool result;
                if (!string.IsNullOrEmpty(value) && bool.TryParse(value, out result))
                    return result;
                return false;
            }
        }

        /// <summary>
        /// Gets the assembly of an external logger that can be used to store log items in 3rd party systems
        /// </summary>
        public static string ExternalLoggerAssembly
        {
            get
            {
                var value = GetKeyAsNode("/settings/logging/externalLogger");
                return value != null ? value.Attributes["assembly"].Value : "";
            }
        }
        /// <summary>
        /// Gets the type of an external logger that can be used to store log items in 3rd party systems
        /// </summary>
        public static string ExternalLoggerType
        {
            get
            {
                var value = GetKeyAsNode("/settings/logging/externalLogger");
                return value != null ? value.Attributes["type"].Value : "";
            }
        }

        /// <summary>
        /// Long Audit Trail to external log too
        /// </summary>
        public static bool ExternalLoggerLogAuditTrail
        {
            get
            {
                var value = GetKeyAsNode("/settings/logging/externalLogger");
                if (value != null)
                {
                    var logAuditTrail = value.Attributes["logAuditTrail"].Value;
                    bool result;
                    if (!string.IsNullOrEmpty(logAuditTrail) && bool.TryParse(logAuditTrail, out result))
                        return result;
                }
                return false;
            }
        }

        /// <summary>
        /// Keep user alive as long as they have their browser open? Default is true
        /// </summary>
        public static bool KeepUserLoggedIn
        {
            get
            {
                var value = GetKey("/settings/security/keepUserLoggedIn");
                bool result;
                if (!string.IsNullOrEmpty(value) && bool.TryParse(value, out result))
                    return result;
                return true;
            }
        }

        /// <summary>
        /// Show disabled users in the tree in the Users section in the backoffice
        /// </summary>
        public static bool HideDisabledUsersInBackoffice
        {
            get
            {
                string value = GetKey("/settings/security/hideDisabledUsersInBackoffice");
                bool result;
                if (!string.IsNullOrEmpty(value) && bool.TryParse(value, out result))
                    return result;
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the logs will be auto cleaned
        /// </summary>
        /// <value><c>true</c> if logs are to be automatically cleaned; otherwise, <c>false</c></value>
        public static bool AutoCleanLogs
        {
            get
            {
                string value = GetKey("/settings/logging/autoCleanLogs");
                bool result;
                if (!string.IsNullOrEmpty(value) && bool.TryParse(value, out result))
                    return result;
                return false;
            }
        }

        /// <summary>
        /// Gets the value indicating the log cleaning frequency (in miliseconds)
        /// </summary>
        public static int CleaningMiliseconds
        {
            get
            {
                string value = GetKey("/settings/logging/cleaningMiliseconds");
                int result;
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out result))
                    return result;
                return -1;
            }
        }

        public static int MaxLogAge
        {
            get
            {
                string value = GetKey("/settings/logging/maxLogAge");
                int result;
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out result))
                    return result;
                return -1;
            }
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
            get
            {
                try
                {
                    bool result;
                    if (bool.TryParse(GetKey("/settings/requestHandler/useDomainPrefixes"), out result))
                        return result;
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// This will add a trailing slash (/) to urls when in directory url mode
        /// NOTICE: This will always return false if Directory Urls in not active
        /// </summary>
        public static bool AddTrailingSlash
        {
            get
            {
                try
                {
                    if (GlobalSettings.UseDirectoryUrls)
                    {
                        bool result;
                        if (bool.TryParse(GetKey("/settings/requestHandler/addTrailingSlash"), out result))
                            return result;
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco will use ASP.NET MasterPages for rendering instead of its propriatary templating system.
        /// </summary>
        /// <value><c>true</c> if umbraco will use ASP.NET MasterPages; otherwise, <c>false</c>.</value>
        public static bool UseAspNetMasterPages
        {
            get
            {
                try
                {
                    bool result;
                    if (bool.TryParse(GetKey("/settings/templates/useAspNetMasterPages"), out result))
                        return result;
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }


        /// <summary>
        /// Gets a value indicating whether umbraco will attempt to load any skins to override default template files
        /// </summary>
        /// <value><c>true</c> if umbraco will override templates with skins if present and configured <c>false</c>.</value>
        public static bool EnableTemplateFolders
        {
            get
            {
                try
                {
                    bool result;
                    if (bool.TryParse(GetKey("/settings/templates/enableTemplateFolders"), out result))
                        return result;
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

		//TODO: I"m not sure why we need this, need to ask Gareth what the deal is, pretty sure we can remove it or change it, seems like
		// massive overkill.

        /// <summary>
        /// razor DynamicNode typecasting detects XML and returns DynamicXml - Root elements that won't convert to DynamicXml
        /// </summary>
		public static IEnumerable<string> NotDynamicXmlDocumentElements
        {
            get
            {
                try
                {
                    List<string> items = new List<string>();
                    XmlNode root = GetKeyAsNode("/settings/scripting/razor/notDynamicXmlDocumentElements");
                    foreach (XmlNode element in root.SelectNodes(".//element"))
                    {
                        items.Add(element.InnerText);
                    }
                    return items;
                }
                catch
                {
                    return new List<string>() { "p", "div" };
                }
            }
        }

    	private static IEnumerable<RazorDataTypeModelStaticMappingItem> _razorDataTypeModelStaticMapping;
    	private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

		public static IEnumerable<RazorDataTypeModelStaticMappingItem> RazorDataTypeModelStaticMapping
        {
            get
            {
				/*
				<dataTypeModelStaticMappings>
					<mapping dataTypeGuid="ef94c406-9e83-4058-a780-0375624ba7ca">DigibizAdvancedMediaPicker.RazorModel.ModelBinder</mapping>
					<mapping documentTypeAlias="RoomPage" nodeTypeAlias="teaser">DigibizAdvancedMediaPicker.RazorModel.ModelBinder</mapping>
				</dataTypeModelStaticMappings>
				 */

				using (var l = new UpgradeableReadLock(Lock))
				{
					if (_razorDataTypeModelStaticMapping == null)
					{
						l.UpgradeToWriteLock();
				
						List<RazorDataTypeModelStaticMappingItem> items = new List<RazorDataTypeModelStaticMappingItem>();
						XmlNode root = GetKeyAsNode("/settings/scripting/razor/dataTypeModelStaticMappings");
						if (root != null)
						{
							foreach (XmlNode element in root.SelectNodes(".//mapping"))
							{
								string propertyTypeAlias = null, nodeTypeAlias = null;
								Guid? dataTypeGuid = null;
								if (!string.IsNullOrEmpty(element.InnerText))
								{
									if (element.Attributes["dataTypeGuid"] != null)
									{
										dataTypeGuid = (Guid?)new Guid(element.Attributes["dataTypeGuid"].Value);
									}
									if (element.Attributes["propertyTypeAlias"] != null && !string.IsNullOrEmpty(element.Attributes["propertyTypeAlias"].Value))
									{
										propertyTypeAlias = element.Attributes["propertyTypeAlias"].Value;
									}
									if (element.Attributes["nodeTypeAlias"] != null && !string.IsNullOrEmpty(element.Attributes["nodeTypeAlias"].Value))
									{
										nodeTypeAlias = element.Attributes["nodeTypeAlias"].Value;
									}
									items.Add(new RazorDataTypeModelStaticMappingItem()
									{
										DataTypeGuid = dataTypeGuid,
										PropertyTypeAlias = propertyTypeAlias,
										NodeTypeAlias = nodeTypeAlias,
										TypeName = element.InnerText,
										Raw = element.OuterXml
									});
								}
							}
						}

						_razorDataTypeModelStaticMapping = items;
					}

					return _razorDataTypeModelStaticMapping;	
				}				
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
            get
            {
                try
                {
                    bool result;
                    if (bool.TryParse(GetKey("/settings/content/cloneXmlContent"), out result))
                        return result;
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether rich text editor content should be parsed by tidy.
        /// </summary>
        /// <value><c>true</c> if content is parsed; otherwise, <c>false</c>.</value>
        public static bool TidyEditorContent
        {
            get { return bool.Parse(GetKey("/settings/content/TidyEditorContent")); }
        }

        /// <summary>
        /// Gets the encoding type for the tidyied content.
        /// </summary>
        /// <value>The encoding type as string.</value>
        public static string TidyCharEncoding
        {
            get
            {
                string encoding = GetKey("/settings/content/TidyCharEncoding");
                if (String.IsNullOrEmpty(encoding))
                {
                    encoding = "UTF8";
                }
                return encoding;
            }
        }

        /// <summary>
        /// Gets the property context help option, this can either be 'text', 'icon' or 'none'
        /// </summary>
        /// <value>The property context help option.</value>
        public static string PropertyContextHelpOption
        {
            get { return GetKey("/settings/content/PropertyContextHelpOption").ToLower(); }
        }

        public static string DefaultBackofficeProvider
        {
            get
            {
                string defaultProvider = GetKey("/settings/providers/users/DefaultBackofficeProvider");
                if (String.IsNullOrEmpty(defaultProvider))
                    defaultProvider = "UsersMembershipProvider";

                return defaultProvider;
            }
        }

        /// <summary>
        /// Whether to force safe aliases (no spaces, no special characters) at businesslogic level on contenttypes and propertytypes
        /// </summary>
        public static bool ForceSafeAliases
        {
            get
            {
                string forceSafeAlias = GetKey("/settings/content/ForceSafeAliases");
                if (String.IsNullOrEmpty(forceSafeAlias))
                    return true;
                else
                {
                    try
                    {
                        return bool.Parse(forceSafeAlias);
                    }
                    catch
                    {
                        return true;
                    }
                }

            }
        }


        /// <summary>
        /// Gets the allowed image file types.
        /// </summary>
        /// <value>The allowed image file types.</value>
        public static string ImageFileTypes
        {
            get { return GetKey("/settings/content/imaging/imageFileTypes").ToLowerInvariant(); }
        }

        /// <summary>
        /// Gets the allowed script file types.
        /// </summary>
        /// <value>The allowed script file types.</value>
        public static string ScriptFileTypes
        {
            get { return GetKey("/settings/content/scripteditor/scriptFileTypes"); }
        }

        /// <summary>
        /// Gets the duration in seconds to cache queries to umbraco library member and media methods
        /// Default is 1800 seconds (30 minutes)
        /// </summary>
        public static int UmbracoLibraryCacheDuration
        {
            get
            {
                string libraryCacheDuration = GetKey("/settings/content/UmbracoLibraryCacheDuration");
                if (String.IsNullOrEmpty(libraryCacheDuration))
                    return 1800;
                else
                {
                    try
                    {
                        return int.Parse(libraryCacheDuration);
                    }
                    catch
                    {
                        return 1800;
                    }
                }

            }
        }

        /// <summary>
        /// Gets the path to the scripts folder used by the script editor.
        /// </summary>
        /// <value>The script folder path.</value>
        public static string ScriptFolderPath
        {
            get { return GetKey("/settings/content/scripteditor/scriptFolderPath"); }
        }

        /// <summary>
        /// Enabled or disable the script/code editor
        /// </summary>
        public static bool ScriptDisableEditor
        {
            get
            {
                string _tempValue = GetKey("/settings/content/scripteditor/scriptDisableEditor");
                if (_tempValue != String.Empty)
                    return bool.Parse(_tempValue);
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets the graphic headline format - png or gif
        /// </summary>
        /// <value>The graphic headline format.</value>
        public static string GraphicHeadlineFormat
        {
            get { return GetKey("/settings/content/graphicHeadlineFormat"); }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco will ensure unique node naming.
        /// This will ensure that nodes cannot have the same url, but will add extra characters to a url.
        /// ex: existingnodename.aspx would become existingnodename(1).aspx if a node with the same name is found 
        /// </summary>
        /// <value><c>true</c> if umbraco ensures unique node naming; otherwise, <c>false</c>.</value>
        public static bool EnsureUniqueNaming
        {
            get
            {
                try
                {
                    return bool.Parse(GetKey("/settings/content/ensureUniqueNaming"));
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the notification email sender.
        /// </summary>
        /// <value>The notification email sender.</value>
        public static string NotificationEmailSender
        {
            get { return GetKey("/settings/content/notifications/email"); }
        }

        /// <summary>
        /// Gets a value indicating whether notification-emails are HTML.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if html notification-emails are disabled; otherwise, <c>false</c>.
        /// </value>
        public static bool NotificationDisableHtmlEmail
        {
            get
            {
            	var tempValue = GetKey("/settings/content/notifications/disableHtmlEmail");
            	return tempValue != String.Empty && bool.Parse(tempValue);
            }
        }

        /// <summary>
        /// Gets the allowed attributes on images.
        /// </summary>
        /// <value>The allowed attributes on images.</value>
        public static string ImageAllowedAttributes
        {
            get { return GetKey("/settings/content/imaging/allowedAttributes"); }
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
            get
            {
                try
                {
                    return bool.Parse(UrlReplaceCharacters.Attributes.GetNamedItem("removeDoubleDashes").Value);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco will use distributed calls.
        /// This enables umbraco to share cache and content across multiple servers.
        /// Used for load-balancing high-traffic sites.
        /// </summary>
        /// <value><c>true</c> if umbraco uses distributed calls; otherwise, <c>false</c>.</value>
        public static bool UseDistributedCalls
        {
            get
            {
                try
                {
                    return bool.Parse(GetKeyAsNode("/settings/distributedCall").Attributes.GetNamedItem("enable").Value);
                }
                catch
                {
                    return false;
                }
            }
        }


        /// <summary>
        /// Gets the ID of the user with access rights to perform the distributed calls.
        /// </summary>
        /// <value>The distributed call user.</value>
        public static int DistributedCallUser
        {
            get
            {
                try
                {
                    return int.Parse(GetKey("/settings/distributedCall/user"));
                }
                catch
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Gets the html injected into a (x)html page if Umbraco is running in preview mode
        /// </summary>
        public static string PreviewBadge
        {
            get
            {
                try
                {
                    return GetKey("/settings/content/PreviewBadge");
                }
                catch
                {
                    return "<a id=\"umbracoPreviewBadge\" style=\"position: absolute; top: 0; right: 0; border: 0; width: 149px; height: 149px; background: url('{1}/preview/previewModeBadge.png') no-repeat;\" href=\"{0}/endPreview.aspx?redir={2}\"><span style=\"display:none;\">In Preview Mode - click to end</span></a>";
                }
            }
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
        /// These repositories are used by the build-in package installer and uninstaller to install new packages and check for updates.
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
            get
            {
                try
                {
                    return
                        bool.Parse(
                            GetKeyAsNode("/settings/viewstateMoverModule").Attributes.GetNamedItem("enable").Value);
                }
                catch
                {
                    return false;
                }
            }
        }


        /// <summary>
        /// Tells us whether the Xml Content cache is disabled or not
        /// Default is enabled
        /// </summary>
        public static bool IsXmlContentCacheDisabled
        {
            get
            {
                try
                {
                    bool xmlCacheEnabled;
                    string value = GetKey("/settings/content/XmlCacheEnabled");
                    if (bool.TryParse(value, out xmlCacheEnabled))
                        return !xmlCacheEnabled;
                    // Return default
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Check if there's changes to the umbraco.config xml file cache on disk on each request
        /// Makes it possible to updates environments by syncing the umbraco.config file across instances
        /// Relates to http://umbraco.codeplex.com/workitem/30722
        /// </summary>
        public static bool XmlContentCheckForDiskChanges
        {
            get
            {
                try
                {
                    bool checkForDiskChanges;
                    string value = GetKey("/settings/content/XmlContentCheckForDiskChanges");
                    if (bool.TryParse(value, out checkForDiskChanges))
                        return checkForDiskChanges;
                    // Return default
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// If this is enabled, all Umbraco objects will generate data in the preview table (cmsPreviewXml).
        /// If disabled, only documents will generate data.
        /// This feature is useful if anyone would like to see how data looked at a given time
        /// </summary>
        public static bool EnableGlobalPreviewStorage
        {
            get
            {
                try
                {
                    bool globalPreviewEnabled = false;
                    string value = GetKey("/settings/content/GlobalPreviewStorageEnabled");
                    if (bool.TryParse(value, out globalPreviewEnabled))
                        return !globalPreviewEnabled;
                    // Return default
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Whether to use the new 4.1 schema or the old legacy schema
        /// </summary>
        /// <value>
        /// 	<c>true</c> if yes, use the old node/data model; otherwise, <c>false</c>.
        /// </value>
        public static bool UseLegacyXmlSchema
        {
            get
            {
				try
				{
					string value = GetKey("/settings/content/UseLegacyXmlSchema");
					bool result;
					if (!string.IsNullOrEmpty(value) && bool.TryParse(value, out result))
						return result;
					return true;
				}
				catch (Exception)
				{
					//default. TODO: When we change this to a real config section we won't have to worry about parse errors
					// and should handle defaults with unit tests properly.
					return false; 
				}
            }
        }

		[Obsolete("This setting is not used anymore, the only file extensions that are supported are .cs and .vb files")]
    	public static IEnumerable<string> AppCodeFileExtensionsList
    	{
    		get
    		{
    			return (from XmlNode x in AppCodeFileExtensions 
						where !String.IsNullOrEmpty(x.InnerText)
						select x.InnerText).ToList();
    		}
    	}

		[Obsolete("This setting is not used anymore, the only file extensions that are supported are .cs and .vb files")]
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
                value = UmbracoSettingsXmlDoc.CreateElement("appCodeFileExtensions");
                value.AppendChild(XmlHelper.AddTextNode(UmbracoSettingsXmlDoc, "ext", "cs"));
                value.AppendChild(XmlHelper.AddTextNode(UmbracoSettingsXmlDoc, "ext", "vb"));
                return value;
            }
        }

        /// <summary>
        /// Tells us whether the Xml to always update disk cache, when changes are made to content
        /// Default is enabled
        /// </summary>
        public static bool ContinouslyUpdateXmlDiskCache
        {
            get
            {
                try
                {
                    bool updateDiskCache;
                    string value = GetKey("/settings/content/ContinouslyUpdateXmlDiskCache");
                    if (bool.TryParse(value, out updateDiskCache))
                        return updateDiskCache;
                    // Return default
                    return false;
                }
                catch
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Tells us whether to use a splash page while umbraco is initializing content. 
        /// If not, requests are queued while umbraco loads content. For very large sites (+10k nodes) it might be usefull to 
        /// have a splash page
        /// Default is disabled
        /// </summary>
        public static bool EnableSplashWhileLoading
        {
            get
            {
                try
                {
                    bool updateDiskCache;
                    string value = GetKey("/settings/content/EnableSplashWhileLoading");
                    if (bool.TryParse(value, out updateDiskCache))
                        return updateDiskCache;
                    // Return default
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        private static bool? _resolveUrlsFromTextString;
        public static bool ResolveUrlsFromTextString
        {
            get
            {
                if (_resolveUrlsFromTextString == null)
                {
                    try
                    {
                        bool enableDictionaryFallBack;
                        var value = GetKey("/settings/content/ResolveUrlsFromTextString");
                        if (value != null)
                            if (bool.TryParse(value, out enableDictionaryFallBack))
                                _resolveUrlsFromTextString = enableDictionaryFallBack;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("Could not load /settings/content/ResolveUrlsFromTextString from umbracosettings.config:\r\n {0}",
                                ex.Message);

                        // set url resolving to true (default (legacy) behavior) to ensure we don't keep writing to trace
                        _resolveUrlsFromTextString = true;
                    }
                }
                return _resolveUrlsFromTextString == true;
            }
        }

        /// <summary>
        /// Configuration regarding webservices
        /// </summary>
        /// <remarks>Put in seperate class for more logik/seperation</remarks>
        internal class WebServices
        {
            /// <summary>
            /// Gets a value indicating whether this <see cref="WebServices"/> is enabled.
            /// </summary>
            /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
            public static bool Enabled
            {
                get
                {
                    try
                    {
                        return
                            bool.Parse(GetKeyAsNode("/settings/webservices").Attributes.GetNamedItem("enabled").Value);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            #region "Webservice configuration"

            /// <summary>
            /// Gets the document service users who have access to use the document web service
            /// </summary>
            /// <value>The document service users.</value>
            public static string[] DocumentServiceUsers
            {
                get
                {
                    try
                    {
                        return GetKey("/settings/webservices/documentServiceUsers").Split(',');
                    }
                    catch
                    {
                        return new string[0];
                    }
                }
            }

            /// <summary>
            /// Gets the file service users who have access to use the file web service
            /// </summary>
            /// <value>The file service users.</value>
            public static string[] FileServiceUsers
            {
                get
                {
                    try
                    {
                        return GetKey("/settings/webservices/fileServiceUsers").Split(',');
                    }
                    catch
                    {
                        return new string[0];
                    }
                }
            }


            /// <summary>
            /// Gets the folders used by the file web service
            /// </summary>
            /// <value>The file service folders.</value>
            public static string[] FileServiceFolders
            {
                get
                {
                    try
                    {
                        return GetKey("/settings/webservices/fileServiceFolders").Split(',');
                    }
                    catch
                    {
                        return new string[0];
                    }
                }
            }

            /// <summary>
            /// Gets the member service users who have access to use the member web service
            /// </summary>
            /// <value>The member service users.</value>
            public static string[] MemberServiceUsers
            {
                get
                {
                    try
                    {
                        return GetKey("/settings/webservices/memberServiceUsers").Split(',');
                    }
                    catch
                    {
                        return new string[0];
                    }
                }
            }

            /// <summary>
            /// Gets the stylesheet service users who have access to use the stylesheet web service
            /// </summary>
            /// <value>The stylesheet service users.</value>
            public static string[] StylesheetServiceUsers
            {
                get
                {
                    try
                    {
                        return GetKey("/settings/webservices/stylesheetServiceUsers").Split(',');
                    }
                    catch
                    {
                        return new string[0];
                    }
                }
            }

            /// <summary>
            /// Gets the template service users who have access to use the template web service
            /// </summary>
            /// <value>The template service users.</value>
            public static string[] TemplateServiceUsers
            {
                get
                {
                    try
                    {
                        return GetKey("/settings/webservices/templateServiceUsers").Split(',');
                    }
                    catch
                    {
                        return new string[0];
                    }
                }
            }

            /// <summary>
            /// Gets the media service users who have access to use the media web service
            /// </summary>
            /// <value>The media service users.</value>
            public static string[] MediaServiceUsers
            {
                get
                {
                    try
                    {
                        return GetKey("/settings/webservices/mediaServiceUsers").Split(',');
                    }
                    catch
                    {
                        return new string[0];
                    }
                }
            }


            /// <summary>
            /// Gets the maintenance service users who have access to use the maintance web service
            /// </summary>
            /// <value>The maintenance service users.</value>
            public static string[] MaintenanceServiceUsers
            {
                get
                {
                    try
                    {
                        return GetKey("/settings/webservices/maintenanceServiceUsers").Split(',');
                    }
                    catch
                    {
                        return new string[0];
                    }
                }
            }

            #endregion
        }
    }
}