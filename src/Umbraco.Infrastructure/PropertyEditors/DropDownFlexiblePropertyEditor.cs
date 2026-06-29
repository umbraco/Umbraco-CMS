// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a flexible drop-down property editor used in Umbraco for selecting values from a configurable list.
/// This editor allows for dynamic configuration of options and supports various selection scenarios.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.DropDownListFlexible,
    ValueEditorIsReusable = true)]
public class DropDownFlexiblePropertyEditor : DataEditor, IValueSchemaProvider
{
    private readonly IIOHelper _ioHelper;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DropDownFlexiblePropertyEditor"/> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">Factory used to create data value editors for property values.</param>
    /// <param name="ioHelper">Helper for IO operations, such as file and path handling.</param>
    /// <param name="configurationEditorJsonSerializer">Serializer for handling JSON configuration of the editor.</param>
    public DropDownFlexiblePropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(IEnumerable<string>);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration) => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = new JsonArray("array", "null"),
        ["items"] = new JsonObject
        {
            ["type"] = "string",
        },
        ["description"] = "Array of selected values from dropdown",
    };

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultipleValueEditor>(Attribute!);

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new DropDownFlexibleConfigurationEditor(_ioHelper, _configurationEditorJsonSerializer);
}
