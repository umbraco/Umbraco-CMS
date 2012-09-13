using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

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
        
		// CURRENT UMBRACO VERSION ID
    	private const string CurrentUmbracoVersion = "4.9.0";

    	private static string _reservedUrlsCache;
        private static string _reservedPathsCache;
        private static StartsWithContainer _reservedList = new StartsWithContainer();

        #endregion

    	/// <summary>
        /// Gets the reserved urls from web.config.
        /// </summary>
        /// <value>The reserved urls.</value>
        public static string ReservedUrls
        {
            get
            {
            	return ConfigurationManager.AppSettings.ContainsKey("umbracoReservedUrls") 
					? ConfigurationManager.AppSettings["umbracoReservedUrls"] 
					: string.Empty;
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
            	return ConfigurationManager.AppSettings.ContainsKey("umbracoReservedPaths") 
					? ConfigurationManager.AppSettings["umbracoReservedPaths"] 
					: string.Empty;
            }
        }

        /// <summary>
        /// Gets the name of the content XML file.
        /// </summary>
        /// <value>The content XML.</value>
        public static string ContentXmlFile
        {
            get
            {                
				return ConfigurationManager.AppSettings.ContainsKey("umbracoContentXML")
					? ConfigurationManager.AppSettings["umbracoContentXML"]
					: string.Empty;	                    
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
					: string.Empty;	                    
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
					? ConfigurationManager.AppSettings["umbracoPath"]
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
		/// </remarks>
    	internal static string MvcArea
    	{
    		get 
			{ 
				if (Path.IsNullOrWhiteSpace())
				{
					throw new InvalidOperationException("Cannot create an MVC Area path without the umbracoPath specified");
				}
				return Path.TrimStart('~').TrimStart('/').Replace('/', '-').Trim();
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
        public static string DbDsn
        {
            get
            {
				return ConfigurationManager.AppSettings.ContainsKey("umbracoDbDSN")
					? ConfigurationManager.AppSettings["umbracoDbDSN"]
					: string.Empty;	          				
            }
            set
            {
                if (DbDsn != value)
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
        /// Saves a setting into the configuration file.
        /// </summary>
        /// <param name="key">Key of the setting to be saved.</param>
        /// <param name="value">Value of the setting to be saved.</param>
        internal static void SaveSetting(string key, string value)
        {
            var webConfig = new WebConfigurationFileMap();
            var vDirs = webConfig.VirtualDirectories;
            var vDir = FullpathToRoot;
            foreach (VirtualDirectoryMapping v in webConfig.VirtualDirectories)
            {
                if (v.IsAppRoot)
                {
                    vDir = v.PhysicalDirectory;
                }
            }
            
            var doc = new XmlDocument();
            doc.Load(String.Concat(vDir, "web.config"));
            var root = doc.DocumentElement;
            var setting = doc.SelectSingleNode(String.Concat("//appSettings/add[@key='", key, "']"));
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
        public static string DisableXsltExtensions
        {
            get
            {
				return ConfigurationManager.AppSettings.ContainsKey("umbracoDisableXsltExtensions")
					? ConfigurationManager.AppSettings["umbracoDisableXsltExtensions"]
					: string.Empty;	   
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
				return ConfigurationManager.AppSettings.ContainsKey("umbracoEditXhtmlMode")
					? ConfigurationManager.AppSettings["umbracoEditXhtmlMode"]
					: string.Empty;	   
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
				return ConfigurationManager.AppSettings.ContainsKey("umbracoProfileUrl")
					? ConfigurationManager.AppSettings["umbracoProfileUrl"]
					: string.Empty;	   
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
        public static string CurrentVersion
        {
            get
            {
                // change this to be hardcoded in the binary
                return CurrentUmbracoVersion;
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

			//The url should be cleaned up before checking:
			// * If it doesn't contain an '.' in the path then we assume it is a path based URL, if that is the case we should add an trailing '/' because all of our reservedPaths use a trailing '/'
			// * We shouldn't be comparing the query at all
			var pathPart = url.Split('?')[0];
			if (!pathPart.Contains(".") && !pathPart.EndsWith("/"))
			{
				pathPart += "/";
			}

            // return true if url starts with an element of the reserved list
			return _reservedList.StartsWith(pathPart.ToLowerInvariant());
        }

		/// <summary>
		/// Structure that checks in logarithmic time
		/// if a given string starts with one of the added keys.
		/// </summary>
		private class StartsWithContainer
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

					////ensure both have a / on the end
					//part = part.EndsWith("/") ? part : part + "/"; 
					//whole = whole.EndsWith("/") ? whole : whole + "/";
					//if (part.Length >= whole.Length)
					//    return _stringComparer.Compare(part, whole);

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



    
}
