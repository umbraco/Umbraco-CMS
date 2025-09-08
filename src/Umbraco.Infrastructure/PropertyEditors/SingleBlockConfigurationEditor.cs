// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Infrastructure.PropertyEditors;

internal sealed class SingleBlockConfigurationEditor : ConfigurationEditor<SingleBlockConfiguration>
{
    public SingleBlockConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
