// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the listview value editor.
/// </summary>
public class ListViewConfigurationEditor : ConfigurationEditor<ListViewConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListViewConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public ListViewConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
