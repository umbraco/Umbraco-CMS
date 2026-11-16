// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a decimal range with an optional minimum and maximum value, where <c>null</c> means "no bound".
/// </summary>
public class DecimalRange
{
    /// <summary>
    /// Gets or sets the minimum value of the range, or <c>null</c> for no minimum.
    /// </summary>
    public decimal? Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum value of the range, or <c>null</c> for no maximum.
    /// </summary>
    public decimal? Max { get; set; }
}
