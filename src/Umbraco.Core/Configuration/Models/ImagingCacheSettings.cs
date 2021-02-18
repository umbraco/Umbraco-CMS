// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for image cache settings.
    /// </summary>
    public class ImagingCacheSettings
    {
        /// <summary>
        /// Gets or sets a value for the browser image cache maximum age.
        /// </summary>
        public TimeSpan BrowserMaxAge { get; set; } = TimeSpan.FromDays(7);

        /// <summary>
        /// Gets or sets a value for the image cache maximum age.
        /// </summary>
        public TimeSpan CacheMaxAge { get; set; } = TimeSpan.FromDays(365);

        /// <summary>
        /// Gets or sets a value for length of the cached name.
        /// </summary>
        public uint CachedNameLength { get; set; } = 8;

        /// <summary>
        /// Gets or sets a value for the cache folder.
        /// </summary>
        public string CacheFolder { get; set; } = Path.Combine("..", "umbraco", "mediacache");
    }
}
