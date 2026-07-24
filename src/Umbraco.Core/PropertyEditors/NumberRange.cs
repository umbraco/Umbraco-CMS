// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a numeric range with an optional minimum and maximum value, where <c>null</c> means "no bound".
/// </summary>
public class NumberRange
{
    /// <summary>
    /// Gets or sets the minimum value of the range, or <c>null</c> for no minimum.
    /// </summary>
    public int? Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum value of the range, or <c>null</c> for no maximum.
    /// </summary>
    public int? Max { get; set; }
}
