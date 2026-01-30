// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration editor for the multi URL picker property editor.
/// </summary>
public class MultiUrlPickerConfigurationEditor : ConfigurationEditor<MultiUrlPickerConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiUrlPickerConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public MultiUrlPickerConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
