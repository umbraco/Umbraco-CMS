// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed class BlockGridConfigurationEditor : ConfigurationEditor<BlockGridConfiguration>
{
    public BlockGridConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
    {

    }

}
