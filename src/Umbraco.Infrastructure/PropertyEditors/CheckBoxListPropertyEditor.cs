// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     A property editor to allow multiple checkbox selection of pre-defined items.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.CheckBoxList,
    ValueType = ValueTypes.Text, // We use the Text value type to ensure we don't run out of storage space in the database field with large lists with multiple values selected.
    ValueEditorIsReusable = true)]
public class CheckBoxListPropertyEditor : DataEditor, IValueSchemaProvider
{
    private readonly IIOHelper _ioHelper;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    /// <summary>
    ///     The constructor will setup the property editor based on the attribute if one is found
    /// </summary>
    public CheckBoxListPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
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
        ["description"] = "Array of selected values",
    };

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ValueListConfigurationEditor(_ioHelper, _configurationEditorJsonSerializer);

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultipleValueEditor>(Attribute!);
}
