// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed class BlockListConfigurationEditor : ConfigurationEditor<BlockListConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockListConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">An <see cref="IIOHelper"/> instance used for file operations.</param>
    public BlockListConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
