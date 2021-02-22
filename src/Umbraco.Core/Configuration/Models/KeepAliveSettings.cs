// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for keep alive settings.
    /// </summary>
    public class KeepAliveSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the keep alive task is disabled.
        /// </summary>
        public bool DisableKeepAliveTask { get; set; } = false;

        /// <summary>
        /// Gets a value for the keep alive ping URL.
        /// </summary>
        public string KeepAlivePingUrl => "{umbracoApplicationUrl}/api/keepalive/ping";
    }
}
