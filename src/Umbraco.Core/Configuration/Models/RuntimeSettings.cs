// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Typed configuration options for runtime settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigRuntime)]
public class RuntimeSettings
{
    /// <summary>
    /// Gets or sets the runtime mode.
    /// </summary>
    [DefaultValue(RuntimeMode.BackofficeDevelopment)]
    public RuntimeMode Mode { get; set; } = RuntimeMode.BackofficeDevelopment;

    /// <summary>
    /// Gets or sets a value for the maximum query string length.
    /// </summary>
    public int? MaxQueryStringLength { get; set; }

    /// <summary>
    ///     Gets or sets a value for the maximum request length in kb.
    /// </summary>
    public int? MaxRequestLength { get; set; }
}
