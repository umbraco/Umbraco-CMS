// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for healthchecks settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigHealthChecks)]
public class HealthChecksSettings
{
    /// <summary>
    ///     Gets or sets a value for the collection of healthchecks that are disabled.
    /// </summary>
    public IEnumerable<DisabledHealthCheckSettings> DisabledChecks { get; set; } =
        Enumerable.Empty<DisabledHealthCheckSettings>();

    /// <summary>
    ///     Gets or sets a value for the healthcheck notification settings.
    /// </summary>
    public HealthChecksNotificationSettings Notification { get; set; } = new();
}
