using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Routing;
using System.Web.Security;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Security;

namespace Umbraco.Core.Configuration
{
    //NOTE: Do not expose this class ever until we cleanup all configuration including removal of static classes, etc...
    // we have this two tasks logged:
    // http://issues.umbraco.org/issue/U4-58
    // http://issues.umbraco.org/issue/U4-115	

    //TODO:  Replace checking for if the app settings exist and returning an empty string, instead return the defaults!

    /// <summary>
    /// The GlobalSettings Class contains general settings information for the entire Umbraco instance based on information from  web.config appsettings 
    /// </summary>
    internal class GlobalSettings
    {

        #region Private static fields

        private static Version _version;
        private static readonly object Locker = new object();
        //make this volatile so that we can ensure thread safety with a double check lock
    	private static volatile string _reservedUrlsCache;
        private static string _reservedPathsCache;
        private static HashSet<string> _reservedList = new HashSet<string>();
        private static string _reservedPaths;
        private static string _reservedUrls;
        //ensure the built on (non-changeable) reserved paths are there at all times
        private const string StaticReservedPaths = "~/app_plugins/,~/install/,";
        private const string StaticReservedUrls = "~/config/splashes/booting.aspx,~/config/splashes/noNodes.aspx,~/VSEnterpriseHelper.axd,";
        #endregion

        /// <summary>
        /// Used in unit testing to reset all config items that were set with property setters (i.e. did not come from config)
        /// </summary>
        private static void ResetInternal()
        {
            _reservedUrlsCache = null;
            _reservedPaths = null;
            _reservedUrls = null;
            HasSmtpServer = null;
        }

        /// <summary>
        /// Resets settings that were set programmatically, to their initial values.
        /// </summary>
        /// <remarks>To be used in unit tests.</remarks>
        internal static void Reset()
        {
            ResetInternal();
        }

        public static bool HasSmtpServerConfigured(string appPath)
        {
            if (HasSmtpServer.HasValue) return HasSmtpServer.Value;

            var config = WebConfigurationManager.OpenWebConfiguration(appPath);
            var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");
            if (settings == null || settings.Smtp == null) return false;
            if (settings.Smtp.SpecifiedPickupDirectory != null && string.IsNullOrEmpty(settings.Smtp.SpecifiedPickupDirectory.PickupDirectoryLocation) == false)
                return true;
            if (settings.Smtp.Network != null && string.IsNullOrEmpty(settings.Smtp.Network.Host) == false)
                return true;
            return false;
        }

        /// <summary>
        /// For testing only
        /// </summary>
        internal static bool? HasSmtpServer { get; set; }

        /// <summary>
        /// Gets the reserved urls from web.config.
        /// </summary>
        /// <value>The reserved urls.</value>
        public static string ReservedUrls
        {
            get
            {                
                if (_reservedUrls == null)
                {
                    var urls = ConfigurationManager.AppSettings.ContainsKey("umbracoReservedUrls")
                                   ? ConfigurationManager.AppSettings["umbracoReservedUrls"]
                                   : string.Empty;

                    //ensure the built on (non-changeable) reserved paths are there at all times
                    _reservedUrls = StaticReservedUrls + urls;    
                }
                return _reservedUrls;
            }
            internal set { _reservedUrls = value; }
        }

        /// <summary>
        /// Gets the reserved paths from web.config
        /// </summary>
        /// <value>The reserved paths.</value>
        public static string ReservedPaths
        {
            get
            {
                if (_reservedPaths == null)
                {
                    var reservedPaths = StaticReservedPaths;
                    //always add the umbraco path to the list
                    if (ConfigurationManager.AppSettings.ContainsKey("umbracoPath")
                        && !ConfigurationManager.AppSettings["umbracoPath"].IsNullOrWhiteSpace())
                    {
                        reservedPaths += ConfigurationManager.AppSettings["umbracoPath"].EnsureEndsWith(',');
                    }

                    var allPaths = ConfigurationManager.AppSettings.ContainsKey("umbracoReservedPaths")
                                    ? ConfigurationManager.AppSettings["umbracoReservedPaths"]
                                    : string.Empty;

                    _reservedPaths = reservedPaths + allPaths;
                }
                return _reservedPaths;
            }
            internal set { _reservedPaths = value; }
        }

        /// <summary>
        /// Gets the name of the content XML file.
        /// </summary>
        /// <value>The content XML.</value>
        /// <remarks>
        /// Defaults to ~/App_Data/umbraco.config
        /// </remarks>
        public static string ContentXmlFile
        {
            get
            {
                return ConfigurationManager.AppSettings.ContainsKey("umbracoContentXML")
                    ? ConfigurationManager.AppSettings["umbracoContentXML"]
                    : "~/App_Data/umbraco.config";
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
                return ConfigurationManager.AppSettings.ContainsKey("umbracoStorageDirectory")
                    ? ConfigurationManager.AppSettings["umbracoStorageDirectory"]
                    : "~/App_Data";
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
                return ConfigurationManager.AppSettings.ContainsKey("umbracoPath")
                    ? IOHelper.ResolveUrl(ConfigurationManager.AppSettings["umbracoPath"])
                    : string.Empty;
            }
        }

        /// <summary>
        /// This returns the string of the MVC Area route.
        /// </summary>
        /// <remarks>
        /// THIS IS TEMPORARY AND SHOULD BE REMOVED WHEN WE MIGRATE/UPDATE THE CONFIG SETTINGS TO BE A REAL CONFIG SECTION
        /// AND SHOULD PROBABLY BE HANDLED IN A MORE ROBUST WAY.
        /// 
        /// This will return the MVC area that we will route all custom routes through like surface controllers, etc...
        /// We will use the 'Path' (default ~/umbraco) to create it but since it cannot contain '/' and people may specify a path of ~/asdf/asdf/admin
        /// we will convert the '/' to '-' and use that as the path. its a bit lame but will work.
		/// 
        /// We also make sure that the virtual directory (SystemDirectories.Root) is stripped off first, otherwise we'd end up with something
        /// like "MyVirtualDirectory-Umbraco" instead of just "Umbraco".
        /// </remarks>
        public static string UmbracoMvcArea
        {
            get
            {
                if (Path.IsNullOrWhiteSpace())
                {
                    throw new InvalidOperationException("Cannot create an MVC Area path without the umbracoPath specified");
                }
                var path = Path;
                if (path.StartsWith(SystemDirectories.Root)) // beware of TrimStart, see U4-2518
                    path = path.Substring(SystemDirectories.Root.Length);
			    return path.TrimStart('~').TrimStart('/').Replace('/', '-').Trim().ToLower();
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
        [Obsolete("Use System.Configuration.ConfigurationManager.ConnectionStrings[\"umbracoDbDSN\"] instead")]
        public static string DbDsn
        {
            get
            {
                var settings = ConfigurationManager.ConnectionStrings[Constants.System.UmbracoConnectionName];
                var connectionString = string.Empty;

                if (settings != null)
                {
                    connectionString = settings.ConnectionString;

                    // The SqlCe connectionString is formatted slightly differently, so we need to update it
                    if (settings.ProviderName.Contains("SqlServerCe"))
                        connectionString = string.Format("datalayer=SQLCE4Umbraco.SqlCEHelper,SQLCE4Umbraco;{0}", connectionString);
                }

                return connectionString;
            }
            set
            {
                if (DbDsn != value)
                {
                    if (value.ToLower().Contains("SQLCE4Umbraco.SqlCEHelper".ToLower()))
                    {
                        ApplicationContext.Current.DatabaseContext.ConfigureEmbeddedDatabaseConnection();
                    }
                    else
                    {
                        ApplicationContext.Current.DatabaseContext.ConfigureDatabaseConnection(value);
                    } 
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
                return ConfigurationManager.AppSettings.ContainsKey("umbracoConfigurationStatus")
                    ? ConfigurationManager.AppSettings["umbracoConfigurationStatus"]
                    : string.Empty;
            }
            set
            {
                SaveSetting("umbracoConfigurationStatus", value);
            }
        }
        
        /// <summary>
        /// Gets or sets the Umbraco members membership providers' useLegacyEncoding state. This will return a boolean
        /// </summary>
        /// <value>The useLegacyEncoding status.</value>
        public static bool UmbracoMembershipProviderLegacyEncoding
        {
            get
            {
                return IsConfiguredMembershipProviderUsingLegacyEncoding(Constants.Conventions.Member.UmbracoMemberProviderName);
            }
            set
            {
                SetMembershipProvidersLegacyEncoding(Constants.Conventions.Member.UmbracoMemberProviderName, value);
            }
        }
        
        /// <summary>
        /// Gets or sets the Umbraco users membership providers' useLegacyEncoding state. This will return a boolean
        /// </summary>
        /// <value>The useLegacyEncoding status.</value>
        public static bool UmbracoUsersMembershipProviderLegacyEncoding
        {
            get
            {
                return IsConfiguredMembershipProviderUsingLegacyEncoding(UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider);
            }
            set
            {
                SetMembershipProvidersLegacyEncoding(UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider, value);
            }
        }
		
        /// <summary>
        /// Saves a setting into the configuration file.
        /// </summary>
        /// <param name="key">Key of the setting to be saved.</param>
        /// <param name="value">Value of the setting to be saved.</param>
        internal static void SaveSetting(string key, string value)
        {
            var fileName = IOHelper.MapPath(string.Format("{0}/web.config", SystemDirectories.Root));
            var xml = XDocument.Load(fileName, LoadOptions.PreserveWhitespace);

            var appSettings = xml.Root.DescendantsAndSelf("appSettings").Single();

            // Update appSetting if it exists, or else create a new appSetting for the given key and value
            var setting = appSettings.Descendants("add").FirstOrDefault(s => s.Attribute("key").Value == key);
            if (setting == null)
                appSettings.Add(new XElement("add", new XAttribute("key", key), new XAttribute("value", value)));
            else
                setting.Attribute("value").Value = value;

            xml.Save(fileName, SaveOptions.DisableFormatting);
            ConfigurationManager.RefreshSection("appSettings");
        }

        /// <summary>
        /// Removes a setting from the configuration file.
        /// </summary>
        /// <param name="key">Key of the setting to be removed.</param>
        internal static void RemoveSetting(string key)
        {
            var fileName = IOHelper.MapPath(string.Format("{0}/web.config", SystemDirectories.Root));
            var xml = XDocument.Load(fileName, LoadOptions.PreserveWhitespace);

            var appSettings = xml.Root.DescendantsAndSelf("appSettings").Single();
            var setting = appSettings.Descendants("add").FirstOrDefault(s => s.Attribute("key").Value == key);

            if (setting != null)
            {
                setting.Remove();
                xml.Save(fileName, SaveOptions.DisableFormatting);
                ConfigurationManager.RefreshSection("appSettings");
            }        
        }

        private static void SetMembershipProvidersLegacyEncoding(string providerName, bool useLegacyEncoding)
        {
            //check if this can even be configured.
            var membershipProvider = Membership.Providers[providerName] as MembershipProviderBase;
            if (membershipProvider == null)
            {
                return;
            }
            if (membershipProvider.GetType().Namespace == "umbraco.providers.members")
            {
                //its the legacy one, this cannot be changed
                return;
            }

            var webConfigFilename = IOHelper.MapPath(string.Format("{0}/web.config", SystemDirectories.Root));
            var webConfigXml = XDocument.Load(webConfigFilename, LoadOptions.PreserveWhitespace);

            var membershipConfigs = webConfigXml.XPathSelectElements("configuration/system.web/membership/providers/add").ToList();

            if (membershipConfigs.Any() == false) 
                return;

            var provider = membershipConfigs.SingleOrDefault(c => c.Attribute("name") != null && c.Attribute("name").Value == providerName);

            if (provider == null) 
                return;

            provider.SetAttributeValue("useLegacyEncoding", useLegacyEncoding);
            
            webConfigXml.Save(webConfigFilename, SaveOptions.DisableFormatting);
        }

        private static bool IsConfiguredMembershipProviderUsingLegacyEncoding(string providerName)
        {
            //check if this can even be configured.
            var membershipProvider = Membership.Providers[providerName] as MembershipProviderBase;
            if (membershipProvider == null)
            {
                return false;
            }

            return membershipProvider.UseLegacyEncoding;            
        }

        /// <summary>
        /// Gets the full path to root.
        /// </summary>
        /// <value>The fullpath to root.</value>
        public static string FullpathToRoot
        {
            get { return IOHelper.GetRootDirectorySafe(); }
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
                    if (HttpContext.Current != null)
                    {
                        return HttpContext.Current.IsDebuggingEnabled;
                    }
                    //go and get it from config directly
                    var section = ConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
                    return section != null && section.Debug;
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
        [Obsolete("Do not use this, it is no longer in use and will be removed from the codebase in future versions")]
        internal static bool Configured
        {
            get
            {
                try
                {
                    string configStatus = ConfigurationStatus;
                    string currentVersion = UmbracoVersion.GetSemanticVersion().ToSemanticString();


                    if (currentVersion != configStatus)
                    {
                        LogHelper.Debug<GlobalSettings>("CurrentVersion different from configStatus: '" + currentVersion + "','" + configStatus + "'");
                    }


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
                try
                {
                    return int.Parse(ConfigurationManager.AppSettings["umbracoVersionCheckPeriod"]);
                }
                catch
                {
                    return 7;
                }
            }
        }

        /// <summary>
        /// Returns a string value to determine if umbraco should disbable xslt extensions
        /// </summary>
        /// <value><c>"true"</c> if version xslt extensions are disabled, otherwise, <c>"false"</c></value>
        [Obsolete("This is no longer used and will be removed from the codebase in future releases")]
        public static string DisableXsltExtensions
        {
            get
            {
                return ConfigurationManager.AppSettings.ContainsKey("umbracoDisableXsltExtensions")
                    ? ConfigurationManager.AppSettings["umbracoDisableXsltExtensions"]
                    : "false";
            }
        }

        internal static bool ContentCacheXmlStoredInCodeGen
        {
            get { return LocalTempStorageLocation == LocalTempStorage.AspNetTemp; }
        }

        /// <summary>
        /// This is the location type to store temporary files such as cache files or other localized files for a given machine
        /// </summary>
        /// <remarks>
        /// Currently used for the xml cache file and the plugin cache files
        /// </remarks>
        internal static LocalTempStorage LocalTempStorageLocation
        {
            get
            {
                //there's a bunch of backwards compat config checks here....

                //This is the current one
                if (ConfigurationManager.AppSettings.ContainsKey("umbracoLocalTempStorage"))
                {
                    return Enum<LocalTempStorage>.Parse(ConfigurationManager.AppSettings["umbracoLocalTempStorage"]);
                }

                //This one is old
                if (ConfigurationManager.AppSettings.ContainsKey("umbracoContentXMLStorage"))
                {
                    return Enum<LocalTempStorage>.Parse(ConfigurationManager.AppSettings["umbracoContentXMLStorage"]);
                }

                //This one is older
                if (ConfigurationManager.AppSettings.ContainsKey("umbracoContentXMLUseLocalTemp"))
                {
                    return bool.Parse(ConfigurationManager.AppSettings["umbracoContentXMLUseLocalTemp"]) 
                        ? LocalTempStorage.AspNetTemp 
                        : LocalTempStorage.Default;
                }
                return LocalTempStorage.Default;
            }
        }

        /// <summary>
        /// Returns a string value to determine if umbraco should use Xhtml editing mode in the wysiwyg editor
        /// </summary>
        /// <value><c>"true"</c> if Xhtml mode is enable, otherwise, <c>"false"</c></value>
        [Obsolete("This is no longer used and will be removed from the codebase in future releases")]
        public static string EditXhtmlMode
        {
            get { return "true"; }
        }

        /// <summary>
        /// Gets the default UI language.
        /// </summary>
        /// <value>The default UI language.</value>
        public static string DefaultUILanguage
        {
            get
            {
                return ConfigurationManager.AppSettings.ContainsKey("umbracoDefaultUILanguage")
                    ? ConfigurationManager.AppSettings["umbracoDefaultUILanguage"]
                    : string.Empty;
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
                //the default will be 'profiler'
                return ConfigurationManager.AppSettings.ContainsKey("umbracoProfileUrl")
                    ? ConfigurationManager.AppSettings["umbracoProfileUrl"]
                    : "profiler";
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
                try
                {
                    return bool.Parse(ConfigurationManager.AppSettings["umbracoHideTopLevelNodeFromPath"]);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the current version.
        /// </summary>
        /// <value>The current version.</value>
        [Obsolete("Use Umbraco.Core.Configuration.UmbracoVersion.Current instead", false)]
        public static string CurrentVersion
        {
            get { return UmbracoVersion.GetSemanticVersion().ToSemanticString(); }
        }

        /// <summary>
        /// Gets the major version number.
        /// </summary>
        /// <value>The major version number.</value>
        [Obsolete("Use Umbraco.Core.Configuration.UmbracoVersion.Current instead", false)]
        public static int VersionMajor
        {
            get
            {
                return UmbracoVersion.Current.Major;
            }
        }

        /// <summary>
        /// Gets the minor version number.
        /// </summary>
        /// <value>The minor version number.</value>
        [Obsolete("Use Umbraco.Core.Configuration.UmbracoVersion.Current instead", false)]
        public static int VersionMinor
        {
            get
            {
                return UmbracoVersion.Current.Minor;
            }
        }

        /// <summary>
        /// Gets the patch version number.
        /// </summary>
        /// <value>The patch version number.</value>
        [Obsolete("Use Umbraco.Core.Configuration.UmbracoVersion.Current instead", false)]
        public static int VersionPatch
        {
            get
            {
                return UmbracoVersion.Current.Build;
            }
        }

        /// <summary>
        /// Gets the version comment (like beta or RC).
        /// </summary>
        /// <value>The version comment.</value>
        [Obsolete("Use Umbraco.Core.Configuration.UmbracoVersion.Current instead", false)]
        public static string VersionComment
        {
            get
            {
                return Umbraco.Core.Configuration.UmbracoVersion.CurrentComment;
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

        public static bool RequestIsInUmbracoApplication(HttpContextBase context)
        {
            return context.Request.Path.ToLower().IndexOf(IOHelper.ResolveUrl(SystemDirectories.Umbraco).ToLower()) > -1;
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

                var versionDoc = new XmlDocument();
                var versionReader = new XmlTextReader(IOHelper.MapPath(SystemDirectories.Umbraco + "/version.xml"));
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

                return license;
            }
        }

        /// <summary>
        /// Determines whether the current request is reserved based on the route table and 
        /// whether the specified URL is reserved or is inside a reserved path.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpContext"></param>
        /// <param name="routes">The route collection to lookup the request in</param>
        /// <returns></returns>
        public static bool IsReservedPathOrUrl(string url, HttpContextBase httpContext, RouteCollection routes)
        {
            if (httpContext == null) throw new ArgumentNullException("httpContext");
            if (routes == null) throw new ArgumentNullException("routes");

            //check if the current request matches a route, if so then it is reserved.
            var route = routes.GetRouteData(httpContext);
            if (route != null)
                return true;

            //continue with the standard ignore routine
            return IsReservedPathOrUrl(url);
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
            if (_reservedUrlsCache == null)
            {
                lock (Locker)
                {
                    if (_reservedUrlsCache == null)
                    {
                        // store references to strings to determine changes
                        _reservedPathsCache = GlobalSettings.ReservedPaths;
                        _reservedUrlsCache = GlobalSettings.ReservedUrls;
                        
                        // add URLs and paths to a new list
                        var newReservedList = new HashSet<string>();
                        foreach (var reservedUrlTrimmed in _reservedUrlsCache
                            .Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim().ToLowerInvariant())
                            .Where(x => x.IsNullOrWhiteSpace() == false)
                            .Select(reservedUrl => IOHelper.ResolveUrl(reservedUrl).Trim().EnsureStartsWith("/"))
                            .Where(reservedUrlTrimmed => reservedUrlTrimmed.IsNullOrWhiteSpace() == false))
                        {
                            newReservedList.Add(reservedUrlTrimmed);
                        }

                        foreach (var reservedPathTrimmed in _reservedPathsCache
                            .Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim().ToLowerInvariant())
                            .Where(x => x.IsNullOrWhiteSpace() == false)
                            .Select(reservedPath => IOHelper.ResolveUrl(reservedPath).Trim().EnsureStartsWith("/").EnsureEndsWith("/"))
                            .Where(reservedPathTrimmed => reservedPathTrimmed.IsNullOrWhiteSpace() == false))
                        {
                            newReservedList.Add(reservedPathTrimmed);
                        }

                        // use the new list from now on
                        _reservedList = newReservedList;
                    }
                }
            }

            //The url should be cleaned up before checking:
            // * If it doesn't contain an '.' in the path then we assume it is a path based URL, if that is the case we should add an trailing '/' because all of our reservedPaths use a trailing '/'
            // * We shouldn't be comparing the query at all
            var pathPart = url.Split(new[] {'?'}, StringSplitOptions.RemoveEmptyEntries)[0].ToLowerInvariant();
            if (pathPart.Contains(".") == false)
            {
                pathPart = pathPart.EnsureEndsWith('/');
            }

            // return true if url starts with an element of the reserved list
            return _reservedList.Any(x => pathPart.InvariantStartsWith(x));
        }

      

    }




}
