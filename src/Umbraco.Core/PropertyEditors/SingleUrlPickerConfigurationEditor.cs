// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration editor for the URL picker property editor holding a single link.
/// </summary>
public class SingleUrlPickerConfigurationEditor : ConfigurationEditor<SingleUrlPickerConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleUrlPickerConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public SingleUrlPickerConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
