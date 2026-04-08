// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration editor for the element picker property editor.
/// </summary>
internal sealed class ElementPickerConfigurationEditor : ConfigurationEditor<ElementPickerConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementPickerConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public ElementPickerConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
