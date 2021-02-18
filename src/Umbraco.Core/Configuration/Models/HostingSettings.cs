// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for hosting settings.
    /// </summary>
    public class HostingSettings
    {
        /// <summary>
        /// Gets or sets a value for the application virtual path.
        /// </summary>
        public string ApplicationVirtualPath { get; set; }

        /// <summary>
        /// Gets or sets a value for the location of temporary files.
        /// </summary>
        public LocalTempStorage LocalTempStorageLocation { get; set; } = LocalTempStorage.Default;

        /// <summary>
        /// Gets or sets a value indicating whether umbraco is running in [debug mode].
        /// </summary>
        /// <value><c>true</c> if [debug mode]; otherwise, <c>false</c>.</value>
        public bool Debug { get; set; } = false;
    }
}
