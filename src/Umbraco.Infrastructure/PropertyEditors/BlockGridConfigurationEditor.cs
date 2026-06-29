// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed class BlockGridConfigurationEditor : ConfigurationEditor<BlockGridConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockGridConfigurationEditor"/> class with the specified IO helper.
    /// </summary>
    /// <param name="ioHelper">An implementation of <see cref="IIOHelper"/> used to assist with file and directory operations.</param>
    public BlockGridConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
    {

    }

}
