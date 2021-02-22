// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for image resize settings.
    /// </summary>
    public class ImagingResizeSettings
    {
        /// <summary>
        /// Gets or sets a value for the maximim resize width.
        /// </summary>
        public int MaxWidth { get; set; } = 5000;

        /// <summary>
        /// Gets or sets a value for the maximim resize height.
        /// </summary>
        public int MaxHeight { get; set; } = 5000;
    }
}
