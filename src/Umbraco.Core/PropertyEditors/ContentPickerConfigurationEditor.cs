// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration editor for the content picker property editor.
/// </summary>
internal sealed class ContentPickerConfigurationEditor : ConfigurationEditor<ContentPickerConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentPickerConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public ContentPickerConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
