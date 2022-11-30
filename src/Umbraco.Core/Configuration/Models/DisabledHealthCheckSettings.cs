// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for disabled healthcheck settings.
/// </summary>
public class DisabledHealthCheckSettings
{
    /// <summary>
    ///     Gets or sets a value for the healthcheck Id to disable.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets a value for the date the healthcheck was disabled.
    /// </summary>
    public DateTime DisabledOn { get; set; }

    /// <summary>
    ///     Gets or sets a value for Id of the user that disabled the healthcheck.
    /// </summary>
    public int DisabledBy { get; set; }
}
