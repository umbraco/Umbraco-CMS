// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the multinode picker value editor.
/// </summary>
public class MultiNodePickerConfigurationEditor : ConfigurationEditor<MultiNodePickerConfiguration>
{
    public MultiNodePickerConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
