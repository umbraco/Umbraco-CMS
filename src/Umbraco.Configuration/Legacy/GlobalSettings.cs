using System;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Xml.Linq;
using Umbraco.Composing;
using Umbraco.Configuration;
using Umbraco.Core.IO;

namespace Umbraco.Core.Configuration.Legacy
{
    // TODO:  Replace checking for if the app settings exist and returning an empty string, instead return the defaults!
    // TODO: need to massively cleanup these configuration classes

    /// <summary>
    /// The GlobalSettings Class contains general settings information for the entire Umbraco instance based on information from  web.config appsettings
    /// </summary>
    public class GlobalSettings : IGlobalSettings
    {

        // TODO these should not be static
        private static string _reservedPaths;
        private static string _reservedUrls;

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
        }

        /// <summary>
        /// Resets settings that were set programmatically, to their initial values.
        /// </summary>
        /// <remarks>To be used in unit tests.</remarks>
        internal static void Reset()
        {
            ResetInternal();
        }


        public bool IsSmtpServerConfigured
        {
            get
            {
                var smtpSettings = SmtpSettings;

                if (smtpSettings is null) return false;

                if (!(smtpSettings.From is null)) return true;
                if (!(smtpSettings.Host is null)) return true;
                if (!(smtpSettings.PickupDirectoryLocation is null)) return true;

                return false;
            }
        }

        public ISmtpSettings SmtpSettings
        {
            get
            {
                var smtpSection = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as ConfigurationSection;
                if (smtpSection is null) return null;

                var result = new SmtpSettings();
                var from = smtpSection.ElementInformation.Properties["from"];
                if (@from != null
                    && @from.Value is string fromPropValue
                    && string.IsNullOrEmpty(fromPropValue) == false
                    && !string.Equals("noreply@example.com", fromPropValue, StringComparison.OrdinalIgnoreCase))
                {
                    result.From = fromPropValue;
                }

                var specifiedPickupDirectorySection = ConfigurationManager.GetSection("system.net/mailSettings/smtp/specifiedPickupDirectory") as ConfigurationSection;
                var pickupDirectoryLocation = specifiedPickupDirectorySection?.ElementInformation.Properties["pickupDirectoryLocation"];
                if (pickupDirectoryLocation != null
                    && pickupDirectoryLocation.Value is string pickupDirectoryLocationPropValue
                    && string.IsNullOrEmpty(pickupDirectoryLocationPropValue) == false)
                {
                    result.PickupDirectoryLocation = pickupDirectoryLocationPropValue;
                }

                // SmtpClient can magically read the section system.net/mailSettings/smtp/network, witch is always
                // null if we use ConfigurationManager.GetSection. SmtpSection does not exist in .Net Standard
                var smtpClient = new SmtpClient();

                result.Host = smtpClient.Host;
                result.Port = smtpClient.Port;

                return result;

            }
        }

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
                var umbPath = ConfigurationManager.AppSettings.ContainsKey(Constants.AppSettings.UmbracoPath) && !ConfigurationManager.AppSettings[Constants.AppSettings.UmbracoPath].IsNullOrWhiteSpace()
                    ? ConfigurationManager.AppSettings[Constants.AppSettings.UmbracoPath]
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
                SaveSetting(Constants.AppSettings.ConfigurationStatus, value, Current.IOHelper); //TODO remove
            }
        }

        /// <summary>
        /// Saves a setting into the configuration file.
        /// </summary>
        /// <param name="key">Key of the setting to be saved.</param>
        /// <param name="value">Value of the setting to be saved.</param>
        internal static void SaveSetting(string key, string value, IIOHelper ioHelper)
        {
            var fileName = ioHelper.MapPath("~/web.config");
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
                    var val = ConfigurationManager.AppSettings[Constants.AppSettings.VersionCheckPeriod];
                    if (!(val is null))
                    {
                        return int.Parse(val);
                    }
                }
                catch
                {
                    // Ignore
                }
                return 7;
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

        private string _umbracoMediaPath = null;
        public string UmbracoMediaPath => GetterWithDefaultValue(Constants.AppSettings.UmbracoMediaPath, "~/media", ref _umbracoMediaPath);

        private string _umbracoScriptsPath = null;
        public string UmbracoScriptsPath => GetterWithDefaultValue(Constants.AppSettings.UmbracoScriptsPath, "~/scripts", ref _umbracoScriptsPath);

        private string _umbracoCssPath = null;
        public string UmbracoCssPath => GetterWithDefaultValue(Constants.AppSettings.UmbracoCssPath, "~/css", ref _umbracoCssPath);

        private string _umbracoPath = null;
        public string UmbracoPath => GetterWithDefaultValue(Constants.AppSettings.UmbracoPath, "~/umbraco", ref _umbracoPath);

        private bool _installMissingDatabase;
        public bool InstallMissingDatabase => GetterWithDefaultValue("Umbraco.Core.RuntimeState.InstallMissingDatabase", false, ref _installMissingDatabase);

        private bool _installEmptyDatabase;
        public bool InstallEmptyDatabase => GetterWithDefaultValue("Umbraco.Core.RuntimeState.InstallEmptyDatabase", false, ref _installEmptyDatabase);

        private bool _disableElectionForSingleServer;
        public bool DisableElectionForSingleServer => GetterWithDefaultValue(Constants.AppSettings.DisableElectionForSingleServer, false, ref _disableElectionForSingleServer);

        private string _registerType;
        public string RegisterType => GetterWithDefaultValue(Constants.AppSettings.RegisterType, string.Empty, ref _registerType);

        private string _databaseFactoryServerVersion;
        public string DatabaseFactoryServerVersion => GetterWithDefaultValue(Constants.AppSettings.Debug.DatabaseFactoryServerVersion, string.Empty, ref _databaseFactoryServerVersion);



        private string _iconsPath;
        /// <summary>
        /// Gets the path to folder containing the icons used in the umbraco backoffice (/umbraco/assets/icons by default).
        /// </summary>
        /// <value>The icons path.</value>
        public string IconsPath => GetterWithDefaultValue(Constants.AppSettings.IconsPath, $"{UmbracoPath}/assets/icons", ref _iconsPath);

        private string _mainDomLock;

        public string MainDomLock => GetterWithDefaultValue(Constants.AppSettings.MainDomLock, string.Empty, ref _mainDomLock);

        private T GetterWithDefaultValue<T>(string appSettingKey, T defaultValue, ref T backingField)
        {
            if (backingField != null) return backingField;

            if (ConfigurationManager.AppSettings.ContainsKey(appSettingKey))
            {
                try
                {
                    var value = ConfigurationManager.AppSettings[appSettingKey];

                    backingField = (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    /* ignore and use default value */
                    backingField = defaultValue;
                }
            }
            else
            {
                backingField = defaultValue;
            }

            return backingField;
        }

        /// <summary>
        /// Gets the path to the razor file used when no published content is available.
        /// </summary>
        public string NoNodesViewPath
        {
            get
            {
                var configuredValue = ConfigurationManager.AppSettings[Constants.AppSettings.NoNodesViewPath];
                if (!string.IsNullOrWhiteSpace(configuredValue))
                {
                    return configuredValue;
                }

                return "~/config/splashes/NoNodes.cshtml";
            }
        }
    }
}
