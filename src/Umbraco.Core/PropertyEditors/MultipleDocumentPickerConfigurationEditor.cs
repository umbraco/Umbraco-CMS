// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration editor for the multiple document picker property editor.
/// </summary>
internal sealed class MultipleDocumentPickerConfigurationEditor : ConfigurationEditor<MultipleDocumentPickerConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleDocumentPickerConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public MultipleDocumentPickerConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
