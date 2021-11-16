using System;
using System.Configuration;
using System.Linq;
using System.Net.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Configuration
{
    // TODO:  Replace checking for if the app settings exist and returning an empty string, instead return the defaults!
    // TODO: need to massively cleanup these configuration classes

    /// <summary>
    /// The GlobalSettings Class contains general settings information for the entire Umbraco instance based on information from  web.config appsettings
    /// </summary>
    public class GlobalSettings : IGlobalSettings
    {
        private string _localTempPath;

        // TODO these should not be static
        private static string _reservedPaths;
        private static string _reservedUrls;
        private static int _sqlWriteLockTimeOut;

        //ensure the built on (non-changeable) reserved paths are there at all times
        internal const string StaticReservedPaths = "~/app_plugins/,~/install/,~/mini-profiler-resources/,"; //must end with a comma!
        internal const string StaticReservedUrls = "~/config/splashes/noNodes.aspx,~/.well-known,"; //must end with a comma!

        /// <summary>
        /// Used in unit testing to reset all config items that were set with property setters (i.e. did not come from config)
        /// </summary>
        private static void ResetInternal()
        {
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
        /// Gets the reserved URLs from web.config.
        /// </summary>
        /// <value>The reserved URLs.</value>
        public string ReservedUrls
        {
            get
            {
                if (_reservedUrls != null) return _reservedUrls;

                var urls = ConfigurationManager.AppSettings.ContainsKey(Constants.AppSettings.ReservedUrls)
                    ? ConfigurationManager.AppSettings[Constants.AppSettings.ReservedUrls]
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
                var umbPath = ConfigurationManager.AppSettings.ContainsKey(Constants.AppSettings.Path) && !ConfigurationManager.AppSettings[Constants.AppSettings.Path].IsNullOrWhiteSpace()
                    ? ConfigurationManager.AppSettings[Constants.AppSettings.Path]
                    : "~/umbraco";
                //always add the umbraco path to the list
                reservedPaths += umbPath.EnsureEndsWith(',');

                var allPaths = ConfigurationManager.AppSettings.ContainsKey(Constants.AppSettings.ReservedPaths)
                    ? ConfigurationManager.AppSettings[Constants.AppSettings.ReservedPaths]
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
                return ConfigurationManager.AppSettings.ContainsKey(Constants.AppSettings.ContentXML)
                    ? ConfigurationManager.AppSettings[Constants.AppSettings.ContentXML]
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
                return ConfigurationManager.AppSettings.ContainsKey(Constants.AppSettings.Path)
                    ? IOHelper.ResolveUrl(ConfigurationManager.AppSettings[Constants.AppSettings.Path])
                    : string.Empty;
            }
        }

        /// <summary>
        /// Gets the path to folder containing the icons used in the umbraco backoffice (/umbraco/assets/icons by default).
        /// </summary>
        /// <value>The icons path.</value>
        public string IconsPath
        {
            get
            {
                return ConfigurationManager.AppSettings.ContainsKey(Constants.AppSettings.IconsPath)
                    ? IOHelper.ResolveUrl(Constants.AppSettings.IconsPath)
                    : $"{Path}/assets/icons";
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
                    return int.Parse(ConfigurationManager.AppSettings[Constants.AppSettings.TimeOutInMinutes]);
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
                    return int.Parse(ConfigurationManager.AppSettings[Constants.AppSettings.VersionCheckPeriod]);
                }
                catch
                {
                    return 7;
                }
            }
        }

        /// <inheritdoc />
        public LocalTempStorage LocalTempStorageLocation
        {
            get
            {
                var setting = ConfigurationManager.AppSettings[Constants.AppSettings.LocalTempStorage];
                if (!string.IsNullOrWhiteSpace(setting))
                    return Enum<LocalTempStorage>.Parse(setting);

                return LocalTempStorage.Default;
            }
        }

        /// <inheritdoc />
        public string LocalTempPath
        {
            get
            {
                if (_localTempPath != null)
                    return _localTempPath;

                switch (LocalTempStorageLocation)
                {
                    case LocalTempStorage.AspNetTemp:
                        return _localTempPath = System.IO.Path.Combine(HttpRuntime.CodegenDir, "UmbracoData");

                    case LocalTempStorage.EnvironmentTemp:

                        // environment temp is unique, we need a folder per site

                        // use a hash
                        // combine site name and application id
                        //  site name is a Guid on Cloud
                        //  application id is eg /LM/W3SVC/123456/ROOT
                        // the combination is unique on one server
                        // and, if a site moves from worker A to B and then back to A...
                        //  hopefully it gets a new Guid or new application id?

                        var siteName = HostingEnvironment.SiteName;
                        var applicationId = HostingEnvironment.ApplicationID; // ie HttpRuntime.AppDomainAppId

                        var hashString = siteName + "::" + applicationId;
                        var hash = hashString.GenerateHash();
                        var siteTemp = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "UmbracoData", hash);

                        return _localTempPath = siteTemp;

                    //case LocalTempStorage.Default:
                    //case LocalTempStorage.Unknown:
                    default:
                        return _localTempPath = IOHelper.MapPath("~/App_Data/TEMP");
                }
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
                return ConfigurationManager.AppSettings.ContainsKey(Constants.AppSettings.DefaultUILanguage)
                    ? ConfigurationManager.AppSettings[Constants.AppSettings.DefaultUILanguage]
                    : string.Empty;
            }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco should hide top level nodes from generated URLs.
        /// </summary>
        /// <value>
        ///     <c>true</c> if umbraco hides top level nodes from URLs; otherwise, <c>false</c>.
        /// </value>
        public bool HideTopLevelNodeFromPath
        {
            get
            {
                try
                {
                    return bool.Parse(ConfigurationManager.AppSettings[Constants.AppSettings.HideTopLevelNodeFromPath]);
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
                    return bool.Parse(ConfigurationManager.AppSettings[Constants.AppSettings.UseHttps]);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns true if TinyMCE scripting sanitization should be applied
        /// </summary>
        /// <remarks>
        /// The default value is false
        /// </remarks>
        public bool SanitizeTinyMce
        {
            get
            {
                try
                {
                    return bool.Parse(ConfigurationManager.AppSettings[Constants.AppSettings.SanitizeTinyMce]);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// An int value representing the time in milliseconds to lock the database for a write operation
        /// </summary>
        /// <remarks>
        /// The default value is 5000 milliseconds
        /// </remarks>
        /// <value>The timeout in milliseconds.</value>
        public int SqlWriteLockTimeOut
        {
            get
            {
                if (_sqlWriteLockTimeOut != default) return _sqlWriteLockTimeOut;

                var timeOut = GetSqlWriteLockTimeoutFromConfigFile(Current.Logger);

                _sqlWriteLockTimeOut = timeOut;
                return _sqlWriteLockTimeOut;
            }
        }

        internal static int GetSqlWriteLockTimeoutFromConfigFile(ILogger logger)
        {
            var timeOut = 5000; // 5 seconds
            var appSettingSqlWriteLockTimeOut = ConfigurationManager.AppSettings[Constants.AppSettings.SqlWriteLockTimeOut];
            if (int.TryParse(appSettingSqlWriteLockTimeOut, out var configuredTimeOut))
            {
                // Only apply this setting if it's not excessively high or low
                const int minimumTimeOut = 100;
                const int maximumTimeOut = 20000;
                if (configuredTimeOut >= minimumTimeOut && configuredTimeOut <= maximumTimeOut) // between 0.1 and 20 seconds
                {
                    timeOut = configuredTimeOut;
                }
                else
                {
                    logger.Warn<GlobalSettings>(
                        $"The `{Constants.AppSettings.SqlWriteLockTimeOut}` setting in web.config is not between the minimum of {minimumTimeOut} ms and maximum of {maximumTimeOut} ms, defaulting back to {timeOut}");
                }
            }

            return timeOut;
        }
    }
}
