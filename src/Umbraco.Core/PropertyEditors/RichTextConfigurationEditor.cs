// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the rich text value editor.
/// </summary>
public class RichTextConfigurationEditor : ConfigurationEditor<RichTextConfiguration>
{
    public RichTextConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
