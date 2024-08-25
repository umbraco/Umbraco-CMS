// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

public class MultiUrlPickerConfigurationEditor : ConfigurationEditor<MultiUrlPickerConfiguration>
{
    public MultiUrlPickerConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
