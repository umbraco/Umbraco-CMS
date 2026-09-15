// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a drop-down property editor holding a single one of the configured values.
/// </summary>
/// <remarks>
/// Stores the same value as <see cref="DropDownFlexiblePropertyEditor" /> and is edited the same way; the two
/// differ in the shape of the value they yield, and in how many values may be selected.
/// </remarks>
[DataEditor(
    Constants.PropertyEditors.Aliases.SingleDropDown,
    ValueEditorIsReusable = true)]
public class SingleDropDownPropertyEditor : DropDownPropertyEditorBase
{
    private readonly IIOHelper _ioHelper;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleDropDownPropertyEditor"/> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">Factory used to create data value editors for property values.</param>
    /// <param name="ioHelper">Helper for IO operations, such as file and path handling.</param>
    /// <param name="configurationEditorJsonSerializer">Serializer for handling JSON configuration of the editor.</param>
    public SingleDropDownPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new SingleDropDownConfigurationEditor(_ioHelper, _configurationEditorJsonSerializer);
}
