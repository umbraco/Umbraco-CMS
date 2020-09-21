using System;
using Umbraco.Core.Configuration.Models.Validation;

namespace Umbraco.Core.Configuration.Models
{
    public class HostingSettings
    {
        public string ApplicationVirtualPath { get; set; }

        // See note on ContentSettings.MacroErrors
        internal string LocalTempStorageLocation { get; set; } = LocalTempStorage.Default.ToString();

        /// <summary>
        /// Gets the configuration for the location of temporary files.
        /// </summary>
        public LocalTempStorage LocalTempStorageLocationValue
        {
            get
            {
                if (Enum.TryParse<LocalTempStorage>(LocalTempStorageLocation, true, out var value))
                {
                    return value;
                }

                // We need to return somethhing valid here as this property is evalulated during start-up, and if there's an error
                // in the configured value it won't be parsed to the enum.
                // At run-time though this default won't be used, as an invalid value will be picked up by HostingSettingsValidator.
                return LocalTempStorage.Default;
            }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco is running in [debug mode].
        /// </summary>
        /// <value><c>true</c> if [debug mode]; otherwise, <c>false</c>.</value>
        public bool Debug { get; set; } = false;
    }
}
