// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration editor for the member picker property editor holding any number of members.
/// </summary>
internal sealed class MultipleMemberPickerConfigurationEditor : ConfigurationEditor<MultipleMemberPickerConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleMemberPickerConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public MultipleMemberPickerConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
