using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Hosting;
using System.Web.Configuration;
using System.Xml;

using umbraco.BusinessLogic;
using umbraco.IO;

namespace umbraco
{
    /// <summary>
    /// The GlobalSettings Class contains general settings information for the entire Umbraco instance based on information from  web.config appsettings 
    /// </summary>
    public class GlobalSettings
    {
        #region Private static fields
        // CURRENT UMBRACO VERSION ID
        private static string _currentVersion = "4.6.1";

        private static string _reservedUrlsCache;
        private static string _reservedPathsCache;
        private static StartsWithContainer _reservedList = new StartsWithContainer();
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalSettings"/> class.
        /// </summary>
        public GlobalSettings()
        {
        }

        /// <summary>
        /// Gets the reserved urls from web.config.
        /// </summary>
        /// <value>The reserved urls.</value>
        public static string ReservedUrls
        {
            get
            {
                if (HttpContext.Current != null)
                    return ConfigurationManager.AppSettings["umbracoReservedUrls"];
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the reserved paths from web.config
        /// </summary>
        /// <value>The reserved paths.</value>
        public static string ReservedPaths
        {
            get
            {
                if (HttpContext.Current != null)
                    return ConfigurationManager.AppSettings["umbracoReservedPaths"];
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the name of the content XML file.
        /// </summary>
        /// <value>The content XML.</value>
        public static string ContentXML
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["umbracoContentXML"];
                }
                catch
                {
                    return String.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the path to the storage directory (/data by default).
        /// </summary>
        /// <value>The storage directory.</value>
        public static string StorageDirectory
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["umbracoStorageDirectory"];
                }
                catch
                {
                    return String.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the path to umbraco's root directory (/umbraco by default).
        /// </summary>
        /// <value>The path.</value>
        public static string Path
        {
            get
            {

                try
                {
                    return IOHelper.ResolveUrl(ConfigurationManager.AppSettings["umbracoPath"]);
                }
                catch
                {
                    return String.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the path to umbraco's client directory (/umbraco_client by default).
        /// This is a relative path to the Umbraco Path as it always must exist beside the 'umbraco'
        /// folder since the CSS paths to images depend on it.
        /// </summary>
        /// <value>The path.</value>
        public static string ClientPath
        {
            get
            {
                return Path + "/../umbraco_client";
            }
        }

        /// <summary>
        /// Gets the database connection string
        /// </summary>
        /// <value>The database connection string.</value>
        public static string DbDSN
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["umbracoDbDSN"];
                }
                catch
                {
                    return String.Empty;
                }
            }
            set
            {
                if (DbDSN != value)
                {
                    SaveSetting("umbracoDbDSN", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the configuration status. This will return the version number of the currently installed umbraco instance.
        /// </summary>
        /// <value>The configuration status.</value>
        public static string ConfigurationStatus
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["umbracoConfigurationStatus"];
                }
                catch
                {
                    return String.Empty;
                }
            }
            set
            {
                SaveSetting("umbracoConfigurationStatus", value);
            }
        }

        private static AspNetHostingPermissionLevel? m_ApplicationTrustLevel = null;
        public static AspNetHostingPermissionLevel ApplicationTrustLevel
        {
            get
            {
                if (!m_ApplicationTrustLevel.HasValue)
                {
                    //set minimum
                    m_ApplicationTrustLevel = AspNetHostingPermissionLevel.None;

                    //determine maximum
                    foreach (AspNetHostingPermissionLevel trustLevel in
                            new AspNetHostingPermissionLevel[] {
                                AspNetHostingPermissionLevel.Unrestricted,
                                AspNetHostingPermissionLevel.High,
                                AspNetHostingPermissionLevel.Medium,
                                AspNetHostingPermissionLevel.Low,
                                AspNetHostingPermissionLevel.Minimal 
                            })
                    {
                        try
                        {
                            new AspNetHostingPermission(trustLevel).Demand();
                            m_ApplicationTrustLevel = trustLevel;
                            break; //we've set the highest permission we can
                        }
                        catch (System.Security.SecurityException)
                        {
                            continue;
                        }                        
                    }
                }
                return m_ApplicationTrustLevel.Value;
            }
        }


        /// <summary>
        /// Forces umbraco to be medium trust compatible
        /// </summary>
        /// <value>If true, umbraco will be medium-trust compatible, no matter what Permission level the server is on.</value>
        public static bool UseMediumTrust
        {
            get
            {
                try
                {
                    if (ApplicationTrustLevel == AspNetHostingPermissionLevel.High || ApplicationTrustLevel == AspNetHostingPermissionLevel.Unrestricted)
                        return false;
                    else
                        return bool.Parse(ConfigurationManager.AppSettings["umbracoUseMediumTrust"]);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Saves a setting into the configuration file.
        /// </summary>
        /// <param name="key">Key of the setting to be saved.</param>
        /// <param name="value">Value of the setting to be saved.</param>
        protected static void SaveSetting(string key, string value)
        {
            WebConfigurationFileMap webConfig = new WebConfigurationFileMap();
            var vDirs = webConfig.VirtualDirectories;
            var vDir = FullpathToRoot;
            foreach (VirtualDirectoryMapping v in webConfig.VirtualDirectories)
            {
                if (v.IsAppRoot)
                {
                    vDir = v.PhysicalDirectory;
                }
            }
            
            XmlDocument doc = new XmlDocument();
            doc.Load(String.Concat(vDir, "web.config"));
            XmlNode root = doc.DocumentElement;
            XmlNode setting = doc.SelectSingleNode(String.Concat("//appSettings/add[@key='", key, "']"));
            setting.Attributes["value"].InnerText = value;
            doc.Save(String.Concat(vDir, "web.config"));
            ConfigurationManager.RefreshSection("appSettings");
        }

        /// <summary>
        /// Gets the full path to root.
        /// </summary>
        /// <value>The fullpath to root.</value>
        public static string FullpathToRoot
        {
            get { return HttpRuntime.AppDomainAppPath; }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco is running in [debug mode].
        /// </summary>
        /// <value><c>true</c> if [debug mode]; otherwise, <c>false</c>.</value>
        public static bool DebugMode
        {
            get
            {
                try
                {
                    return bool.Parse(ConfigurationManager.AppSettings["umbracoDebugMode"]);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current version of umbraco is configured.
        /// </summary>
        /// <value><c>true</c> if configured; otherwise, <c>false</c>.</value>
        public static bool Configured
        {
            get
            {
                try
                {
                    string configStatus = ConfigurationStatus;
                    string currentVersion = CurrentVersion;


                    if (currentVersion != configStatus)
                        Log.Add(LogTypes.Debug, User.GetUser(0), -1,
                                "CurrentVersion different from configStatus: '" + currentVersion + "','" + configStatus +
                                "'");

                    return (configStatus == currentVersion);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the time out in minutes.
        /// </summary>
        /// <value>The time out in minutes.</value>
        public static int TimeOutInMinutes
        {
            get
            {
                try
                {
                    return int.Parse(ConfigurationManager.AppSettings["umbracoTimeOutInMinutes"]);
                }
                catch
                {
                    return 20;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco uses directory urls.
        /// </summary>
        /// <value><c>true</c> if umbraco uses directory urls; otherwise, <c>false</c>.</value>
        public static bool UseDirectoryUrls
        {
            get
            {
                try
                {
                    return bool.Parse(ConfigurationManager.AppSettings["umbracoUseDirectoryUrls"]);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns a string value to determine if umbraco should skip version-checking.
        /// </summary>
        /// <value>The version check period in days (0 = never).</value>
        public static int VersionCheckPeriod
        {
            get
            {
                int versionCheckPeriod = 7;
                if (HttpContext.Current != null)
                {
                    if (int.TryParse(ConfigurationManager.AppSettings["umbracoVersionCheckPeriod"], out versionCheckPeriod))
                        return versionCheckPeriod;

                }
                return versionCheckPeriod;
            }
        }

        /// <summary>
        /// Gets the URL forbitten characters.
        /// </summary>
        /// <value>The URL forbitten characters.</value>
        public static string UrlForbittenCharacters
        {
            get
            {
                if (HttpContext.Current != null)
                    return ConfigurationManager.AppSettings["umbracoUrlForbittenCharacters"];
                return "";
            }
        }

        /// <summary>
        /// Gets the URL space character.
        /// </summary>
        /// <value>The URL space character.</value>
        public static string UrlSpaceCharacter
        {
            get
            {
                if (HttpContext.Current != null)
                    return ConfigurationManager.AppSettings["umbracoUrlSpaceCharacter"];
                return "";
            }
        }

        /// <summary>
        /// Gets the SMTP server IP-address or hostname.
        /// </summary>
        /// <value>The SMTP server.</value>
        public static string SmtpServer
        {
            get
            {
                try
                {
                    System.Net.Configuration.MailSettingsSectionGroup mailSettings = ConfigurationManager.GetSection("system.net/mailSettings") as System.Net.Configuration.MailSettingsSectionGroup;

                    if (mailSettings != null)
                        return mailSettings.Smtp.Network.Host;
                    else
                        return ConfigurationManager.AppSettings["umbracoSmtpServer"];
                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Returns a string value to determine if umbraco should disbable xslt extensions
        /// </summary>
        /// <value><c>"true"</c> if version xslt extensions are disabled, otherwise, <c>"false"</c></value>
        public static string DisableXsltExtensions
        {
            get
            {
                if (HttpContext.Current != null)
                    return ConfigurationManager.AppSettings["umbracoDisableXsltExtensions"];
                return "";
            }
        }

        /// <summary>
        /// Returns a string value to determine if umbraco should use Xhtml editing mode in the wysiwyg editor
        /// </summary>
        /// <value><c>"true"</c> if Xhtml mode is enable, otherwise, <c>"false"</c></value>
        public static string EditXhtmlMode
        {
            get
            {
                if (HttpContext.Current != null)
                    return ConfigurationManager.AppSettings["umbracoEditXhtmlMode"];
                return "";
            }
        }

        /// <summary>
        /// Gets the default UI language.
        /// </summary>
        /// <value>The default UI language.</value>
        public static string DefaultUILanguage
        {
            get
            {
                if (HttpContext.Current != null)
                    return ConfigurationManager.AppSettings["umbracoDefaultUILanguage"];
                return "";
            }
        }

        /// <summary>
        /// Gets the profile URL.
        /// </summary>
        /// <value>The profile URL.</value>
        public static string ProfileUrl
        {
            get
            {
                if (HttpContext.Current != null)
                    return ConfigurationManager.AppSettings["umbracoProfileUrl"];
                return "";
            }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco should hide top level nodes from generated urls.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if umbraco hides top level nodes from urls; otherwise, <c>false</c>.
        /// </value>
        public static bool HideTopLevelNodeFromPath
        {
            get
            {
                if (HttpContext.Current != null)
                    return bool.Parse(ConfigurationManager.AppSettings["umbracoHideTopLevelNodeFromPath"]);
                return false;
            }
        }

        /// <summary>
        /// Gets the current version.
        /// </summary>
        /// <value>The current version.</value>
        public static string CurrentVersion
        {
            get
            {
                // change this to be hardcoded in the binary
                return _currentVersion;
            }
        }

        /// <summary>
        /// Gets the major version number.
        /// </summary>
        /// <value>The major version number.</value>
        public static int VersionMajor
        {
            get
            {
                string[] version = CurrentVersion.Split(".".ToCharArray());
                return int.Parse(version[0]);
            }
        }

        /// <summary>
        /// Gets the minor version number.
        /// </summary>
        /// <value>The minor version number.</value>
        public static int VersionMinor
        {
            get
            {
                string[] version = CurrentVersion.Split(".".ToCharArray());
                return int.Parse(version[1]);
            }
        }

        /// <summary>
        /// Gets the patch version number.
        /// </summary>
        /// <value>The patch version number.</value>
        public static int VersionPatch
        {
            get
            {
                string[] version = CurrentVersion.Split(".".ToCharArray());
                return int.Parse(version[2]);
            }
        }

        /// <summary>
        /// Gets the version comment (like beta or RC).
        /// </summary>
        /// <value>The version comment.</value>
        public static string VersionComment
        {
            get
            {
                string[] version = CurrentVersion.Split(".".ToCharArray());
                if (version.Length > 3)
                    return version[3];
                else
                    return "";
            }
        }


        /// <summary>
        /// Requests the is in umbraco application directory structure.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static bool RequestIsInUmbracoApplication(HttpContext context)
        {
            return context.Request.Path.ToLower().IndexOf(IOHelper.ResolveUrl(SystemDirectories.Umbraco).ToLower()) > -1;
        }

        public static bool RequestIsLiveEditRedirector(HttpContext context)
        {
            return context.Request.Path.ToLower().IndexOf(SystemDirectories.Umbraco.ToLower() + "/liveediting.aspx") > -1;
        }

        /// <summary>
        /// Gets a value indicating whether umbraco should force a secure (https) connection to the backoffice.
        /// </summary>
        /// <value><c>true</c> if [use SSL]; otherwise, <c>false</c>.</value>
        public static bool UseSSL
        {
            get
            {
                try
                {
                    return bool.Parse(ConfigurationManager.AppSettings["umbracoUseSSL"]);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the umbraco license.
        /// </summary>
        /// <value>The license.</value>
        public static string License
        {
            get
            {
                string license =
                    "<A href=\"http://umbraco.org/redir/license\" target=\"_blank\">the open source license MIT</A>. The umbraco UI is freeware licensed under the umbraco license.";
                if (HttpContext.Current != null)
                {
                    XmlDocument versionDoc = new XmlDocument();
                    XmlTextReader versionReader = new XmlTextReader(IOHelper.MapPath(SystemDirectories.Umbraco + "/version.xml"));
                    versionDoc.Load(versionReader);
                    versionReader.Close();

                    // check for license
                    try
                    {
                        string licenseUrl =
                            versionDoc.SelectSingleNode("/version/licensing/licenseUrl").FirstChild.Value;
                        string licenseValidation =
                            versionDoc.SelectSingleNode("/version/licensing/licenseValidation").FirstChild.Value;
                        string licensedTo =
                            versionDoc.SelectSingleNode("/version/licensing/licensedTo").FirstChild.Value;

                        if (licensedTo != "" && licenseUrl != "")
                        {
                            license = "umbraco Commercial License<br/><b>Registered to:</b><br/>" +
                                      licensedTo.Replace("\n", "<br/>") + "<br/><b>For use with domain:</b><br/>" +
                                      licenseUrl;
                        }
                    }
                    catch
                    {
                    }
                }
                return license;
            }
        }


        /// <summary>
        /// Developer method to test if configuration settings are loaded properly.
        /// </summary>
        /// <value><c>true</c> if succesfull; otherwise, <c>false</c>.</value>
        public static bool test
        {
            get
            {
                try
                {
                    HttpContext.Current.Response.Write("ContentXML :" + ContentXML + "\n");
                    HttpContext.Current.Response.Write("DbDSN :" + DbDSN + "\n");
                    HttpContext.Current.Response.Write("DebugMode :" + DebugMode + "\n");
                    HttpContext.Current.Response.Write("DefaultUILanguage :" + DefaultUILanguage + "\n");
                    HttpContext.Current.Response.Write("VersionCheckPeriod :" + VersionCheckPeriod + "\n");
                    HttpContext.Current.Response.Write("DisableXsltExtensions :" + DisableXsltExtensions + "\n");
                    HttpContext.Current.Response.Write("EditXhtmlMode :" + EditXhtmlMode + "\n");
                    HttpContext.Current.Response.Write("HideTopLevelNodeFromPath :" + HideTopLevelNodeFromPath + "\n");
                    HttpContext.Current.Response.Write("Path :" + Path + "\n");
                    HttpContext.Current.Response.Write("ProfileUrl :" + ProfileUrl + "\n");
                    HttpContext.Current.Response.Write("ReservedPaths :" + ReservedPaths + "\n");
                    HttpContext.Current.Response.Write("ReservedUrls :" + ReservedUrls + "\n");
                    HttpContext.Current.Response.Write("StorageDirectory :" + StorageDirectory + "\n");
                    HttpContext.Current.Response.Write("TimeOutInMinutes :" + TimeOutInMinutes + "\n");
                    HttpContext.Current.Response.Write("UrlForbittenCharacters :" + UrlForbittenCharacters + "\n");
                    HttpContext.Current.Response.Write("UrlSpaceCharacter :" + UrlSpaceCharacter + "\n");
                    HttpContext.Current.Response.Write("UseDirectoryUrls :" + UseDirectoryUrls + "\n");
                    return true;
                }
                catch
                {
                }
                return false;
            }
        }


        /// <summary>
        /// Determines whether the specified URL is reserved or is inside a reserved path.
        /// </summary>
        /// <param name="url">The URL to check.</param>
        /// <returns>
        /// 	<c>true</c> if the specified URL is reserved; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsReservedPathOrUrl(string url)
        {
            // check if GlobalSettings.ReservedPaths and GlobalSettings.ReservedUrls are unchanged
            if (!object.ReferenceEquals(_reservedPathsCache, GlobalSettings.ReservedPaths)
                || !object.ReferenceEquals(_reservedUrlsCache, GlobalSettings.ReservedUrls))
            {
                // store references to strings to determine changes
                _reservedPathsCache = GlobalSettings.ReservedPaths;
                _reservedUrlsCache = GlobalSettings.ReservedUrls;

                string _root = SystemDirectories.Root.Trim().ToLower();

                // add URLs and paths to a new list
                StartsWithContainer _newReservedList = new StartsWithContainer();
                foreach (string reservedUrl in _reservedUrlsCache.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    //resolves the url to support tilde chars
                    string reservedUrlTrimmed = IOHelper.ResolveUrl(reservedUrl).Trim().ToLower();
                    if (reservedUrlTrimmed.Length > 0)
                        _newReservedList.Add(reservedUrlTrimmed);
                }

                foreach (string reservedPath in _reservedPathsCache.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    bool trimEnd = !reservedPath.EndsWith("/");
                    //resolves the url to support tilde chars
                    string reservedPathTrimmed = IOHelper.ResolveUrl(reservedPath).Trim().ToLower();

                    if (reservedPathTrimmed.Length > 0)
                        _newReservedList.Add(reservedPathTrimmed + (reservedPathTrimmed.EndsWith("/") ? "" : "/"));
                }

                // use the new list from now on
                _reservedList = _newReservedList;
            }

            string res = "";
            foreach (string st in _reservedList._list.Keys)
                res += st + ",";

            HttpContext.Current.Trace.Write("umbracoGlobalsettings", "reserverd urls: '" + res + "'");

            // return true if url starts with an element of the reserved list
            return _reservedList.StartsWith(url.ToLower());
        }
    }



    /// <summary>
    /// Structure that checks in logarithmic time
    /// if a given string starts with one of the added keys.
    /// </summary>
    public class StartsWithContainer
    {
        /// <summary>Internal sorted list of keys.</summary>
        public SortedList<string, string> _list
            = new SortedList<string, string>(StartsWithComparator.Instance);

        /// <summary>
        /// Adds the specified new key.
        /// </summary>
        /// <param name="newKey">The new key.</param>
        public void Add(string newKey)
        {
            // if the list already contains an element that begins with newKey, return
            if (String.IsNullOrEmpty(newKey) || StartsWith(newKey))
                return;

            // create a new collection, so the old one can still be accessed
            SortedList<string, string> newList
                = new SortedList<string, string>(_list.Count + 1, StartsWithComparator.Instance);

            // add only keys that don't already start with newKey, others are unnecessary
            foreach (string key in _list.Keys)
                if (!key.StartsWith(newKey))
                    newList.Add(key, null);
            // add the new key
            newList.Add(newKey, null);

            // update the list (thread safe, _list was never in incomplete state)
            _list = newList;
        }

        /// <summary>
        /// Checks if the given string starts with any of the added keys.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>true if a key is found that matches the start of target</returns>
        /// <remarks>
        /// Runs in O(s*log(n)), with n the number of keys and s the length of target.
        /// </remarks>
        public bool StartsWith(string target)
        {
            return _list.ContainsKey(target);
        }

        /// <summary>Comparator that tests if a string starts with another.</summary>
        /// <remarks>Not a real comparator, since it is not reflexive. (x==y does not imply y==x)</remarks>
        private sealed class StartsWithComparator : IComparer<string>
        {
            /// <summary>Default string comparer.</summary>
            private readonly static Comparer<string> _stringComparer = Comparer<string>.Default;

            /// <summary>Gets an instance of the StartsWithComparator.</summary>
            public static readonly StartsWithComparator Instance = new StartsWithComparator();

            /// <summary>
            /// Tests if whole begins with all characters of part.
            /// </summary>
            /// <param name="part">The part.</param>
            /// <param name="whole">The whole.</param>
            /// <returns>
            /// Returns 0 if whole starts with part, otherwise performs standard string comparison.
            /// </returns>
            public int Compare(string part, string whole)
            {
                // let the default string comparer deal with null or when part is not smaller then whole
                if (part == null || whole == null || part.Length >= whole.Length)
                    return _stringComparer.Compare(part, whole);

                // loop through all characters that part and whole have in common
                int pos = 0;
                bool match;
                do
                {
                    match = (part[pos] == whole[pos]);
                } while (match && ++pos < part.Length);

                // return result of last comparison
                return match ? 0 : (part[pos] < whole[pos] ? -1 : 1);
            }
        }
    }
}
