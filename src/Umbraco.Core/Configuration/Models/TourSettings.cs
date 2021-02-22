// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for tour settings.
    /// </summary>
    public class TourSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether back-office tours are enabled.
        /// </summary>
        public bool EnableTours { get; set; } = true;
    }
}
