using System;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Xml;
using umbraco.BusinessLogic;

namespace umbraco
{
    /// <summary>
    /// The UmbracoSettings Class contains general settings information for the entire Umbraco instance based on information from the /config/umbracoSettings.config file
    /// </summary>
    public class UmbracoSettings
    {
        // TODO: Remove for launch
        public const string TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME = ""; // "children";
 
        /// <summary>
        /// Gets the umbraco settings document.
        /// </summary>
        /// <value>The _umbraco settings.</value>
        public static XmlDocument _umbracoSettings
        {
            get
            {
                XmlDocument us = (XmlDocument) HttpRuntime.Cache["umbracoSettingsFile"];
                if (us == null)
                    us = ensureSettingsDocument();
                return us;
            }
        }

        private static string _path = GlobalSettings.FullpathToRoot + Path.DirectorySeparatorChar + "config" +
                                      Path.DirectorySeparatorChar;

        private static string _filename = "umbracoSettings.config";

        private static XmlDocument ensureSettingsDocument()
        {
            object settingsFile = HttpRuntime.Cache["umbracoSettingsFile"];

            // Check for language file in cache
            if (settingsFile == null)
            {
                XmlDocument temp = new XmlDocument();
                XmlTextReader settingsReader = new XmlTextReader(_path + _filename);
                try
                {
                    temp.Load(settingsReader);
                    HttpRuntime.Cache.Insert("umbracoSettingsFile", temp,
                                             new CacheDependency(_path + _filename));
                }
                catch (Exception e)
                {
                    Log.Add(LogTypes.Error, new User(0), -1, "Error reading umbracoSettings file: " + e.ToString());
                }
                settingsReader.Close();
                return temp;
            }
            else
                return (XmlDocument) settingsFile;
        }

        private static void save()
        {
            _umbracoSettings.Save(_path + _filename);
        }


        /// <summary>
        /// Selects a xml node in the umbraco settings config file.
        /// </summary>
        /// <param name="Key">The xpath query to the specific node.</param>
        /// <returns>If found, it returns the specific configuration xml node.</returns>
        public static XmlNode GetKeyAsNode(string Key)
        {
            if (Key == null)
                throw new ArgumentException("Key cannot be null");
            ensureSettingsDocument();
            if (_umbracoSettings == null || _umbracoSettings.DocumentElement == null)
                return null;
            return _umbracoSettings.DocumentElement.SelectSingleNode(Key);
        }

        /// <summary>
        /// Gets the value of configuration xml node with the specified key.
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <returns></returns>
        public static string GetKey(string Key)
        {
            ensureSettingsDocument();

            XmlNode node = _umbracoSettings.DocumentElement.SelectSingleNode(Key);
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
        /// Gets a value indicating whether logging is enabled in umbracoSettings.config (/settings/logging/enableLogging).
        /// </summary>
        /// <value><c>true</c> if logging is enabled; otherwise, <c>false</c>.</value>
        public static bool EnableLogging {
            get {
                // We return true if no enable logging element is present in 
                // umbracoSettings (to enable default behaviour when upgrading)
                string m_EnableLogging = GetKey("/settings/logging/enableLogging");
                if (String.IsNullOrEmpty(m_EnableLogging))
                    return true;
                else
                    return bool.Parse(m_EnableLogging); }
        }

        /// <summary>
        /// Gets a value indicating whether logging happens async.
        /// </summary>
        /// <value><c>true</c> if async logging is enabled; otherwise, <c>false</c>.</value>
        public static bool EnableAsyncLogging {
            get {
                string value = GetKey("/settings/logging/enableAsyncLogging");
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
        public static XmlNode DisabledLogTypes {
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
        public static bool UseDomainPrefixes {
            get {
                try {
                    bool result;
                    if (bool.TryParse(GetKey("/settings/requestHandler/useDomainPrefixes"), out result))
                        return result;
                    return false;
                } catch {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco will use ASP.NET MasterPages for rendering instead of its propriatary templating system.
        /// </summary>
        /// <value><c>true</c> if umbraco will use ASP.NET MasterPages; otherwise, <c>false</c>.</value>
        public static bool UseAspNetMasterPages {
            get {
                try {
                    bool result;
                    if (bool.TryParse(GetKey("/settings/templates/useAspNetMasterPages"), out result))
                        return result;
                    return false;
                } catch {
                    return false;
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
            get {
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

        public static string DefaultBackofficeProvider {
            get 
            {
                string defaultProvider = GetKey("/settings/providers/users/DefaultBackofficeProvider");
                if (String.IsNullOrEmpty(defaultProvider))
                    defaultProvider = "UsersMembershipProvider";

                return defaultProvider;
            }
        }

        /// <summary>
        /// Gets the allowed image file types.
        /// </summary>
        /// <value>The allowed image file types.</value>
        public static string ImageFileTypes
        {
            get { return GetKey("/settings/content/imaging/imageFileTypes"); }
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
                string _tempValue = GetKey("/settings/content/notifications/disableHtmlEmail");
                if (_tempValue != String.Empty)
                    return bool.Parse(_tempValue);
                else
                    return false;
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
        /// Gets all repositories registered, and returns them as XmlNodes, containing name, alias and webservice url.
        /// These repositories are used by the build-in package installer and uninstaller to install new packages and check for updates.
        /// All repositories should have a unique alias.
        /// All packages installed from a repository gets the repository alias included in the install information
        /// </summary>
        /// <value>The repository servers.</value>
        public static XmlNode Repositories {
            get {
                try {
                    return GetKeyAsNode("/settings/repositories");
                } catch {
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
        public static bool isXmlContentCacheDisabled
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
        /// Whether to use the new 4.1 schema or the old legacy schema
        /// </summary>
        /// <value>
        /// 	<c>true</c> if yes, use the old node/data model; otherwise, <c>false</c>.
        /// </value>
        public static bool UseLegacyXmlSchema
        {
            get
            {
                string value = GetKey("/settings/content/UseLegacyXmlSchema");
                bool result;
                if (!string.IsNullOrEmpty(value) && bool.TryParse(value, out result))
                    return result;
                return true;
            }
        }

        /// <summary>
        /// Tells us whether the Xml to always update disk cache, when changes are made to content
        /// Default is enabled
        /// </summary>
        public static bool continouslyUpdateXmlDiskCache
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
            public static string[] documentServiceUsers
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
            public static string[] fileServiceUsers
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
            public static string[] fileServiceFolders
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
            public static string[] memberServiceUsers
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
            public static string[] stylesheetServiceUsers
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
            public static string[] templateServiceUsers
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
            public static string[] mediaServiceUsers
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
            public static string[] maintenanceServiceUsers
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