// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the tag value editor.
/// </summary>
public class TagConfigurationEditor : ConfigurationEditor<TagConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TagConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public TagConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
        Field(nameof(TagConfiguration.Group)).Validators.Add(new RequiredValidator());
        Field(nameof(TagConfiguration.StorageType)).Validators.Add(new RequiredValidator());
    }
}
