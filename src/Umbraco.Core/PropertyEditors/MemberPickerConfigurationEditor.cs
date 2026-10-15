// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration editor for the member picker property editor holding a single member.
/// </summary>
internal sealed class MemberPickerConfigurationEditor : ConfigurationEditor<MemberPickerConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberPickerConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public MemberPickerConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
