// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     A <see cref="ConfigurationField" /> for a range value (a <see cref="NumberRange" /> or <see cref="DecimalRange" />)
///     that validates the maximum is not lower than the minimum.
/// </summary>
public sealed class RangeConfigurationField : ConfigurationField
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RangeConfigurationField" /> class.
    /// </summary>
    public RangeConfigurationField() => Validators.Add(new RangeConfigurationValidator());
}
