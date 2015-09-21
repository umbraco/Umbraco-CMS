using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Hosting;
using System.Web.Configuration;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using Umbraco.Core.IO;

namespace umbraco
{
    /// <summary>
    /// The GlobalSettings Class contains general settings information for the entire Umbraco instance based on information from  web.config appsettings 
    /// </summary>
    public class GlobalSettings
    {
		
    	/// <summary>
        /// Gets the reserved urls from web.config.
        /// </summary>
        /// <value>The reserved urls.</value>
        public static string ReservedUrls
        {
            get { return Umbraco.Core.Configuration.GlobalSettings.ReservedUrls; }
        }

        /// <summary>
        /// Gets the reserved paths from web.config
        /// </summary>
        /// <value>The reserved paths.</value>
        public static string ReservedPaths
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.ReservedPaths; }
        }

        /// <summary>
        /// Gets the name of the content XML file.
        /// </summary>
        /// <value>The content XML.</value>
        public static string ContentXML
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.ContentXmlFile; }
        }

        /// <summary>
        /// Gets the path to the storage directory (/data by default).
        /// </summary>
        /// <value>The storage directory.</value>
        public static string StorageDirectory
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.StorageDirectory; }
        }

        /// <summary>
        /// Gets the path to umbraco's root directory (/umbraco by default).
        /// </summary>
        /// <value>The path.</value>
        public static string Path
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.Path; }
        }

        /// <summary>
        /// Gets the path to umbraco's client directory (/umbraco_client by default).
        /// This is a relative path to the Umbraco Path as it always must exist beside the 'umbraco'
        /// folder since the CSS paths to images depend on it.
        /// </summary>
        /// <value>The path.</value>
        public static string ClientPath
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.ClientPath; }
        }

        /// <summary>
        /// Gets the database connection string
        /// </summary>
        /// <value>The database connection string.</value>
        [Obsolete("Use System.ConfigurationManager.ConnectionStrings to get the connection with the key Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName instead")]
        public static string DbDSN
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.DbDsn; }
            set { Umbraco.Core.Configuration.GlobalSettings.DbDsn = value; }
        }

        /// <summary>
        /// Gets or sets the configuration status. This will return the version number of the currently installed umbraco instance.
        /// </summary>
        /// <value>The configuration status.</value>
        public static string ConfigurationStatus
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.ConfigurationStatus; }
            set { Umbraco.Core.Configuration.GlobalSettings.ConfigurationStatus = value; }
        }

        public static AspNetHostingPermissionLevel ApplicationTrustLevel
        {
			get { return Umbraco.Core.SystemUtilities.GetCurrentTrustLevel(); }
        }

        /// <summary>
        /// Saves a setting into the configuration file.
        /// </summary>
        /// <param name="key">Key of the setting to be saved.</param>
        /// <param name="value">Value of the setting to be saved.</param>
        protected static void SaveSetting(string key, string value)
        {
        	Umbraco.Core.Configuration.GlobalSettings.SaveSetting(key, value);
        }

        /// <summary>
        /// Gets the full path to root.
        /// </summary>
        /// <value>The fullpath to root.</value>
        public static string FullpathToRoot
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.FullpathToRoot; }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco is running in [debug mode].
        /// </summary>
        /// <value><c>true</c> if [debug mode]; otherwise, <c>false</c>.</value>
        public static bool DebugMode
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.DebugMode; }
        }

        /// <summary>
        /// Gets a value indicating whether the current version of umbraco is configured.
        /// </summary>
        /// <value><c>true</c> if configured; otherwise, <c>false</c>.</value>
        [Obsolete("Do not use this, it is no longer in use and will be removed from the codebase in future versions")]
        public static bool Configured
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.Configured; }
        }

        /// <summary>
        /// Gets the time out in minutes.
        /// </summary>
        /// <value>The time out in minutes.</value>
        public static int TimeOutInMinutes
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.TimeOutInMinutes; }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco uses directory urls.
        /// </summary>
        /// <value><c>true</c> if umbraco uses directory urls; otherwise, <c>false</c>.</value>
        public static bool UseDirectoryUrls
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.UseDirectoryUrls; }
        }

        /// <summary>
        /// Returns a string value to determine if umbraco should skip version-checking.
        /// </summary>
        /// <value>The version check period in days (0 = never).</value>
        public static int VersionCheckPeriod
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.VersionCheckPeriod; }
        }

        /// <summary>
        /// Gets the URL forbitten characters.
        /// </summary>
        /// <value>The URL forbitten characters.</value>
		[Obsolete("This property is no longer used and will be removed in future versions")]
        public static string UrlForbittenCharacters
        {
			get
			{
				return ConfigurationManager.AppSettings.ContainsKey("umbracoUrlForbittenCharacters")
					? ConfigurationManager.AppSettings["umbracoUrlForbittenCharacters"]
					: string.Empty;
			}
        }

        /// <summary>
        /// Gets the URL space character.
        /// </summary>
        /// <value>The URL space character.</value>
		[Obsolete("This property is no longer used and will be removed in future versions")]
        public static string UrlSpaceCharacter
        {
			get
			{
				return ConfigurationManager.AppSettings.ContainsKey("umbracoUrlSpaceCharacter")
					? ConfigurationManager.AppSettings["umbracoUrlSpaceCharacter"]
					: string.Empty;
			}
        }

        /// <summary>
        /// Gets the SMTP server IP-address or hostname.
        /// </summary>
        /// <value>The SMTP server.</value>
		[Obsolete("This property is no longer used and will be removed in future versions")]
        public static string SmtpServer
        {
			get
			{
				try
				{
					var mailSettings = ConfigurationManager.GetSection("system.net/mailSettings") as System.Net.Configuration.MailSettingsSectionGroup;

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
        [Obsolete("This is no longer used and will be removed from the codebase in future releases")]
        public static string DisableXsltExtensions
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.DisableXsltExtensions; }
        }

        /// <summary>
        /// Returns a string value to determine if umbraco should use Xhtml editing mode in the wysiwyg editor
        /// </summary>
        /// <value><c>"true"</c> if Xhtml mode is enable, otherwise, <c>"false"</c></value>
        [Obsolete("This is no longer used and will be removed from the codebase in future releases")]
        public static string EditXhtmlMode
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.EditXhtmlMode; }
        }

        /// <summary>
        /// Gets the default UI language.
        /// </summary>
        /// <value>The default UI language.</value>
        public static string DefaultUILanguage
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.DefaultUILanguage; }
        }

        /// <summary>
        /// Gets the profile URL.
        /// </summary>
        /// <value>The profile URL.</value>
        public static string ProfileUrl
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.ProfileUrl; }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco should hide top level nodes from generated urls.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if umbraco hides top level nodes from urls; otherwise, <c>false</c>.
        /// </value>
        public static bool HideTopLevelNodeFromPath
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.HideTopLevelNodeFromPath; }
        }

        /// <summary>
        /// Gets the current version.
        /// </summary>
        /// <value>The current version.</value>
        [Obsolete("Use Umbraco.Core.Configuration.UmbracoVersion.Current instead", false)]
        public static string CurrentVersion
        {
			get { return UmbracoVersion.Current.ToString(3); }
        }

        /// <summary>
        /// Gets the major version number.
        /// </summary>
        /// <value>The major version number.</value>
        [Obsolete("Use Umbraco.Core.Configuration.UmbracoVersion.Current instead", false)]
        public static int VersionMajor
        {
			get { return UmbracoVersion.Current.Major; }
        }

        /// <summary>
        /// Gets the minor version number.
        /// </summary>
        /// <value>The minor version number.</value>
        [Obsolete("Use Umbraco.Core.Configuration.UmbracoVersion.Current instead", false)]
        public static int VersionMinor
        {
			get { return UmbracoVersion.Current.Minor; }
        }

        /// <summary>
        /// Gets the patch version number.
        /// </summary>
        /// <value>The patch version number.</value>
        [Obsolete("Use Umbraco.Core.Configuration.UmbracoVersion.Current instead", false)]
        public static int VersionPatch
        {
			get { return UmbracoVersion.Current.Build; }
        }

        /// <summary>
        /// Gets the version comment (like beta or RC).
        /// </summary>
        /// <value>The version comment.</value>
        [Obsolete("Use Umbraco.Core.Configuration.UmbracoVersion.Current instead", false)]
        public static string VersionComment
        {
			get { return UmbracoVersion.CurrentComment; }
        }


        /// <summary>
        /// Requests the is in umbraco application directory structure.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static bool RequestIsInUmbracoApplication(HttpContext context)
        {
        	return Umbraco.Core.Configuration.GlobalSettings.RequestIsInUmbracoApplication(context);
        }
        
        /// <summary>
        /// Gets a value indicating whether umbraco should force a secure (https) connection to the backoffice.
        /// </summary>
        /// <value><c>true</c> if [use SSL]; otherwise, <c>false</c>.</value>
        public static bool UseSSL
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.UseSSL; }
        }

        /// <summary>
        /// Gets the umbraco license.
        /// </summary>
        /// <value>The license.</value>
        public static string License
        {
			get { return Umbraco.Core.Configuration.GlobalSettings.License; }
        }


        /// <summary>
        /// Developer method to test if configuration settings are loaded properly.
        /// </summary>
        /// <value><c>true</c> if succesfull; otherwise, <c>false</c>.</value>
        [Obsolete("This method is no longer used and will be removed in future versions")]
        public static bool test
        {
			get
			{
                var databaseSettings = ConfigurationManager.ConnectionStrings[Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName];
                var dataHelper = DataLayerHelper.CreateSqlHelper(databaseSettings.ConnectionString, false);

				if (HttpContext.Current != null)
				{
					HttpContext.Current.Response.Write("ContentXML :" + ContentXML + "\n");
					HttpContext.Current.Response.Write("DbDSN :" + dataHelper.ConnectionString + "\n");
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
					//HttpContext.Current.Response.Write("UrlForbittenCharacters :" + UrlForbittenCharacters + "\n");
					//HttpContext.Current.Response.Write("UrlSpaceCharacter :" + UrlSpaceCharacter + "\n");
					HttpContext.Current.Response.Write("UseDirectoryUrls :" + UseDirectoryUrls + "\n");
					return true;
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
        	return Umbraco.Core.Configuration.GlobalSettings.IsReservedPathOrUrl(url);
        }
    }



    /// <summary>
    /// Structure that checks in logarithmic time
    /// if a given string starts with one of the added keys.
    /// </summary>
	[Obsolete("Use Umbraco.Core.Configuration.GlobalSettings.StartsWithContainer container instead")]
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
