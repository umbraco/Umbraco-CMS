// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.ComponentModel;
using System.IO;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for image cache settings.
    /// </summary>
    public class ImagingCacheSettings
    {
        internal const string StaticBrowserMaxAge = "7.00:00:00";
        internal const string StaticCacheMaxAge = "365.00:00:00";
        internal const int StaticCacheHashLength = 8;
        internal const string StaticCacheFolder = Constants.SystemDirectories.TempData +  "/MediaCache";

        /// <summary>
        /// Gets or sets a value for the browser image cache maximum age.
        /// </summary>
        [DefaultValue(StaticBrowserMaxAge)]
        public TimeSpan BrowserMaxAge { get; set; } = TimeSpan.Parse(StaticBrowserMaxAge);

        /// <summary>
        /// Gets or sets a value for the image cache maximum age.
        /// </summary>
        [DefaultValue(StaticCacheMaxAge)]
        public TimeSpan CacheMaxAge { get; set; } = TimeSpan.Parse(StaticCacheMaxAge);

        /// <summary>
        /// Gets or sets a value for length of the cached name.
        /// </summary>
        [Obsolete("Use CacheHashLength instead")]
        public uint CachedNameLength { get => CacheHashLength; set { CacheHashLength = value; } }

        /// <summary>
        /// Gets or sets a value for length of the cached name.
        /// </summary>
        [DefaultValue(StaticCacheHashLength)]
        public uint CacheHashLength { get; set; } = StaticCacheHashLength;

        /// <summary>
        /// Gets or sets a value for the cache folder.
        /// </summary>
        [DefaultValue(StaticCacheFolder)]
        public string CacheFolder { get; set; } = StaticCacheFolder;
    }
}
