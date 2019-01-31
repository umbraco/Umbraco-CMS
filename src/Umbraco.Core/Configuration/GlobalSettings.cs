using System;
using System.Configuration;
using System.Linq;
using System.Net.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Security;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Security;

namespace Umbraco.Core.Configuration
{
    // TODO:  Replace checking for if the app settings exist and returning an empty string, instead return the defaults!
    // TODO: need to massively cleanup these configuration classes

    /// <summary>
    /// The GlobalSettings Class contains general settings information for the entire Umbraco instance based on information from  web.config appsettings
    /// </summary>
    public class GlobalSettings : IGlobalSettings
    {

        #region Private static fields

        
        private static string _reservedPaths;
        private static string _reservedUrls;
        //ensure the built on (non-changeable) reserved paths are there at all times
        internal const string StaticReservedPaths = "~/app_plugins/,~/install/,"; //must end with a comma!
        internal const string StaticReservedUrls = "~/config/splashes/noNodes.aspx,~/.well-known,"; //must end with a comma!
        #endregion

        /// <summary>
        /// Used in unit testing to reset all config items that were set with property setters (i.e. did not come from config)
        /// </summary>
        private static void ResetInternal()
        {
            GlobalSettingsExtensions.Reset();
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
            // note: "noreply@example.com" is/was the sample SMTP from email - we'll regard that as "not configured"
            if (settings == null || settings.Smtp == null || "noreply@example.com".Equals(settings.Smtp.From, StringComparison.OrdinalIgnoreCase)) return false;
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
        public string ReservedUrls
        {
            get
            {
                if (_reservedUrls != null) return _reservedUrls;

                var urls = ConfigurationManager.AppSettings.ContainsKey("umbracoReservedUrls")
                    ? ConfigurationManager.AppSettings["umbracoReservedUrls"]
                    : string.Empty;

                //ensure the built on (non-changeable) reserved paths are there at all times
                _reservedUrls = StaticReservedUrls + urls;
                return _reservedUrls;
            }
            internal set => _reservedUrls = value;
        }

        /// <summary>
        /// Gets the reserved paths from web.config
        /// </summary>
        /// <value>The reserved paths.</value>
        public string ReservedPaths
        {
            get
            {
                if (_reservedPaths != null) return _reservedPaths;

                var reservedPaths = StaticReservedPaths;
                var umbPath = ConfigurationManager.AppSettings.ContainsKey("umbracoPath") && !ConfigurationManager.AppSettings["umbracoPath"].IsNullOrWhiteSpace()
                    ? ConfigurationManager.AppSettings["umbracoPath"]
                    : "~/umbraco";
                //always add the umbraco path to the list
                reservedPaths += umbPath.EnsureEndsWith(',');

                var allPaths = ConfigurationManager.AppSettings.ContainsKey("umbracoReservedPaths")
                    ? ConfigurationManager.AppSettings["umbracoReservedPaths"]
                    : string.Empty;

                _reservedPaths = reservedPaths + allPaths;
                return _reservedPaths;
            }
        }

        /// <summary>
        /// Gets the name of the content XML file.
        /// </summary>
        /// <value>The content XML.</value>
        /// <remarks>
        /// Defaults to ~/App_Data/umbraco.config
        /// </remarks>
        public string ContentXmlFile
        {
            get
            {
                return ConfigurationManager.AppSettings.ContainsKey("umbracoContentXML")
                    ? ConfigurationManager.AppSettings["umbracoContentXML"]
                    : "~/App_Data/umbraco.config";
            }
        }
        
        /// <summary>
        /// Gets the path to umbraco's root directory (/umbraco by default).
        /// </summary>
        /// <value>The path.</value>
        public string Path
        {
            get
            {
                return ConfigurationManager.AppSettings.ContainsKey("umbracoPath")
                    ? IOHelper.ResolveUrl(ConfigurationManager.AppSettings["umbracoPath"])
                    : string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the configuration status. This will return the version number of the currently installed umbraco instance.
        /// </summary>
        /// <value>The configuration status.</value>
        public string ConfigurationStatus
        {
            get
            {
                return ConfigurationManager.AppSettings.ContainsKey(Constants.AppSettings.ConfigurationStatus)
                    ? ConfigurationManager.AppSettings[Constants.AppSettings.ConfigurationStatus]
                    : string.Empty;
            }
            set
            {
                SaveSetting(Constants.AppSettings.ConfigurationStatus, value);
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
        /// Gets the time out in minutes.
        /// </summary>
        /// <value>The time out in minutes.</value>
        public int TimeOutInMinutes
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
        /// Returns the number of days that should take place between version checks.
        /// </summary>
        /// <value>The version check period in days (0 = never).</value>
        public int VersionCheckPeriod
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
        /// This is the location type to store temporary files such as cache files or other localized files for a given machine
        /// </summary>
        /// <remarks>
        /// Currently used for the xml cache file and the plugin cache files
        /// </remarks>
        public LocalTempStorage LocalTempStorageLocation
        {
            get
            {
                var setting = ConfigurationManager.AppSettings["umbracoLocalTempStorage"];
                if (!string.IsNullOrWhiteSpace(setting))
                    return Enum<LocalTempStorage>.Parse(setting);

                return LocalTempStorage.Default;
            }
        }

        /// <summary>
        /// Gets the default UI language.
        /// </summary>
        /// <value>The default UI language.</value>
        // ReSharper disable once InconsistentNaming
        public string DefaultUILanguage
        {
            get
            {
                return ConfigurationManager.AppSettings.ContainsKey("umbracoDefaultUILanguage")
                    ? ConfigurationManager.AppSettings["umbracoDefaultUILanguage"]
                    : string.Empty;
            }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco should hide top level nodes from generated urls.
        /// </summary>
        /// <value>
        ///     <c>true</c> if umbraco hides top level nodes from urls; otherwise, <c>false</c>.
        /// </value>
        public bool HideTopLevelNodeFromPath
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
        /// Gets a value indicating whether umbraco should force a secure (https) connection to the backoffice.
        /// </summary>
        public bool UseHttps
        {
            get
            {
                try
                {
                    return bool.Parse(ConfigurationManager.AppSettings["umbracoUseHttps"]);
                }
                catch
                {
                    return false;
                }
            }
        }

    }




}
