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
    /// <summary>
    ///     The default value for whether the notification method is enabled.
    /// </summary>
    internal const bool StaticEnabled = false;

    /// <summary>
    ///     The default verbosity level for health check notifications.
    /// </summary>
    internal const string StaticVerbosity = "Summary"; // Enum

    /// <summary>
    ///     The default value for sending notifications only on failure.
    /// </summary>
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
    public HealthCheckNotificationVerbosity Verbosity { get; set; } = Enum.Parse<HealthCheckNotificationVerbosity>(StaticVerbosity);

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
