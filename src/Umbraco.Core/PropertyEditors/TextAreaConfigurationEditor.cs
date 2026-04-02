// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the textarea value editor.
/// </summary>
public class TextAreaConfigurationEditor : ConfigurationEditor<TextAreaConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextAreaConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public TextAreaConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
