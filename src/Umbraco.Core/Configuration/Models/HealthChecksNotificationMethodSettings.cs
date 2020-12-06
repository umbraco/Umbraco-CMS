// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Core.HealthCheck;

namespace Umbraco.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for healthcheck notification method settings.
    /// </summary>
    public class HealthChecksNotificationMethodSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the health check notification method is enabled.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets or sets a value for the health check notifications reporting verbosity.
        /// </summary>
        public HealthCheckNotificationVerbosity Verbosity { get; set; } = HealthCheckNotificationVerbosity.Summary;

        /// <summary>
        /// Gets or sets a value indicating whether the health check notifications should occur on failures only.
        /// </summary>
        public bool FailureOnly { get; set; } = false;

        /// <summary>
        /// Gets or sets a value providing provider specific settings for the health check notification method.
        /// </summary>
        public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
    }
}
