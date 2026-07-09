// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a textbox property and parameter editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.TextBox,
    ValueEditorIsReusable = true)]
public class TextboxPropertyEditor : DataEditor, IValueSchemaProvider
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TextboxPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">Factory used to create data value editors for property values.</param>
    /// <param name="ioHelper">Helper for IO operations such as path resolution and file access.</param>
    public TextboxPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
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
        if (configuration is TextboxConfiguration textboxConfig && textboxConfig.MaxChars > 0)
        {
            schema["maxLength"] = textboxConfig.MaxChars;
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
        new TextboxConfigurationEditor(_ioHelper);
}
