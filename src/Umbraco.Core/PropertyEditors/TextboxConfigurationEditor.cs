// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the textbox value editor.
/// </summary>
public class TextboxConfigurationEditor : ConfigurationEditor<TextboxConfiguration>
{
    public TextboxConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
