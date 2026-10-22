// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for a multiple textstring value editor.
/// </summary>
internal sealed class MultipleTextStringConfigurationEditor : ConfigurationEditor<MultipleTextStringConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.PropertyEditors.MultipleTextStringConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">An instance of <see cref="IIOHelper"/> used to assist with file system operations.</param>
    public MultipleTextStringConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
