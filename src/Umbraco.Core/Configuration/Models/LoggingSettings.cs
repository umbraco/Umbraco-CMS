// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for logging settings.
    /// </summary>
    public class LoggingSettings
    {
        /// <summary>
        /// Gets or sets a value for the maximum age of a log file.
        /// </summary>
        public TimeSpan MaxLogAge { get; set; } = TimeSpan.FromHours(24);
    }
}
