// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed class ContentPickerConfigurationEditor : ConfigurationEditor<ContentPickerConfiguration>
{
    public ContentPickerConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
