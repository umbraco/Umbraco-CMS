// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the decimal property editor.
/// </summary>
public class DecimalConfiguration
{
    /// <summary>
    ///     Gets or sets the allowed range of the entered value.
    /// </summary>
    [ConfigurationField("validationRange", Type = typeof(RangeConfigurationField))]
    public DecimalRange ValidationRange { get; set; } = new();

    /// <summary>
    ///     Gets or sets the step size between allowed values.
    /// </summary>
    [ConfigurationField("step")]
    public decimal? Step { get; set; }
}
