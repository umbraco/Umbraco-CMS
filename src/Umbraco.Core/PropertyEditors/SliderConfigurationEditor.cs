// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the slider value editor.
/// </summary>
public class SliderConfigurationEditor : ConfigurationEditor<SliderConfiguration>
{
    public SliderConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
