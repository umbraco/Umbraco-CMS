// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the range slider value editor.
/// </summary>
public class RangeSliderConfigurationEditor : ConfigurationEditor<RangeSliderConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RangeSliderConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public RangeSliderConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
