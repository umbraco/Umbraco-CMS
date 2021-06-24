// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for exception filter settings.
    /// </summary>
    [UmbracoOptions(Constants.Configuration.ConfigExceptionFilter)]
    public class ExceptionFilterSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the exception filter is disabled.
        /// </summary>
        public bool Disabled { get; set; } = false;
    }
}
