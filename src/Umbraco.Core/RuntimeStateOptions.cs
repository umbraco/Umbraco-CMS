using System.Configuration;

namespace Umbraco.Core
{
    /// <summary>
    /// Allows configuration of the <see cref="RuntimeState"/> in PreApplicationStart or in appSettings
    /// </summary>
    public static class RuntimeStateOptions
    {
        // configured statically or via app settings
        private static bool BoolSetting(string key, bool missing) => ConfigurationManager.AppSettings[key]?.InvariantEquals("true") ?? missing;

        /// <summary>
        /// If true the RuntimeState will continue the installation sequence when a database is missing
        /// </summary>
        /// <remarks>
        /// In this case it will be up to the implementor that is setting this value to true to take over the bootup/installation sequence
        /// </remarks>
        public static bool InstallMissingDatabase
        {
            get => _installEmptyDatabase ?? BoolSetting("Umbraco.Core.RuntimeState.InstallMissingDatabase", false);
            set => _installEmptyDatabase = value;
        }

        /// <summary>
        /// If true the RuntimeState will continue the installation sequence when a database is available but is empty
        /// </summary>
        /// <remarks>
        /// In this case it will be up to the implementor that is setting this value to true to take over the bootup/installation sequence
        /// </remarks>
        public static bool InstallEmptyDatabase
        {
            get => _installMissingDatabase ?? BoolSetting("Umbraco.Core.RuntimeState.InstallEmptyDatabase", false);
            set => _installMissingDatabase = value;
        }

        private static bool? _installMissingDatabase;
        private static bool? _installEmptyDatabase;
    }
}