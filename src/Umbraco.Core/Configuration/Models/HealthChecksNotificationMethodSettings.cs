// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Umbraco.Cms.Core.HealthChecks;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for healthcheck notification method settings.
/// </summary>
public class HealthChecksNotificationMethodSettings
{
    internal const bool StaticEnabled = false;
    internal const string StaticVerbosity = "Summary"; // Enum
    internal const bool StaticFailureOnly = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the health check notification method is enabled.
    /// </summary>
    [DefaultValue(StaticEnabled)]
    public bool Enabled { get; set; } = StaticEnabled;

    /// <summary>
    ///     Gets or sets a value for the health check notifications reporting verbosity.
    /// </summary>
    [DefaultValue(StaticVerbosity)]
    public HealthCheckNotificationVerbosity Verbosity { get; set; } =
        Enum<HealthCheckNotificationVerbosity>.Parse(StaticVerbosity);

    /// <summary>
    ///     Gets or sets a value indicating whether the health check notifications should occur on failures only.
    /// </summary>
    [DefaultValue(StaticFailureOnly)]
    public bool FailureOnly { get; set; } = StaticFailureOnly;

    /// <summary>
    ///     Gets or sets a value providing provider specific settings for the health check notification method.
    /// </summary>
    public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
}
