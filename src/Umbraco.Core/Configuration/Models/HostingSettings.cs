using System;

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
                return Enum.TryParse<LocalTempStorage>(LocalTempStorageLocation, true, out var value)
                    ? value
                    : LocalTempStorage.Default;
            }
        }

        /// <summary>
        /// Gets a value indicating whether umbraco is running in [debug mode].
        /// </summary>
        /// <value><c>true</c> if [debug mode]; otherwise, <c>false</c>.</value>
        public bool Debug { get; set; } = false;
    }
}
