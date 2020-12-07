// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for NuCache settings.
    /// </summary>
    public class NuCacheSettings
    {
        /// <summary>
        /// Gets or sets a value defining the BTree block size.
        /// </summary>
        public int? BTreeBlockSize { get; set; }
    }
}
