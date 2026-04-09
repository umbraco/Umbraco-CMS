// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a textarea property and parameter editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.TextArea,
    ValueType = ValueTypes.Text,
    ValueEditorIsReusable = true)]
public class TextAreaPropertyEditor : DataEditor, IValueSchemaProvider
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TextAreaPropertyEditor" /> class.
    /// </summary>
    public TextAreaPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(string);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration)
    {
        var schema = new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["type"] = new JsonArray("string", "null"),
        };

        // Add maxLength constraint from configuration if available
        if (configuration is TextAreaConfiguration textAreaConfig && textAreaConfig.MaxChars > 0)
        {
            schema["maxLength"] = textAreaConfig.MaxChars;
        }
        else if (configuration is IDictionary<string, object> configDict &&
                 configDict.TryGetValue("maxChars", out var maxCharsValue))
        {
            if (maxCharsValue is int maxChars && maxChars > 0)
            {
                schema["maxLength"] = maxChars;
            }
        }

        return schema;
    }

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<TextOnlyValueEditor>(Attribute!);

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new TextAreaConfigurationEditor(_ioHelper);
}
