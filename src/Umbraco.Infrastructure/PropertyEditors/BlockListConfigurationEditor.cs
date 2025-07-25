// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed class BlockListConfigurationEditor : ConfigurationEditor<BlockListConfiguration>
{
    public BlockListConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
