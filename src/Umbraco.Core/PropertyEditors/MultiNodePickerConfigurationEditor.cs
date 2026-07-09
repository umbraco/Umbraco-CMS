// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the multinode picker value editor.
/// </summary>
public class MultiNodePickerConfigurationEditor : ConfigurationEditor<MultiNodePickerConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiNodePickerConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public MultiNodePickerConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
