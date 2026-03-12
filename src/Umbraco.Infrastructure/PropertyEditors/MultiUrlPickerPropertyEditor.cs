// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a property editor in Umbraco that enables users to select and manage multiple URLs within a property.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.MultiUrlPicker,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class MultiUrlPickerPropertyEditor : DataEditor, IValueSchemaProvider
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiUrlPickerPropertyEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">Provides file system operations.</param>
    /// <param name="dataValueEditorFactory">Factory for creating data value editors.</param>
    public MultiUrlPickerPropertyEditor(IIOHelper ioHelper, IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <summary>
    /// Gets the property index value factory used by the multi URL picker property editor.
    /// By default, this returns a <see cref="NoopPropertyIndexValueFactory"/>, indicating that no custom indexing is performed for this property editor.
    /// </summary>
    public override IPropertyIndexValueFactory PropertyIndexValueFactory { get; } = new NoopPropertyIndexValueFactory();

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(LinkDisplay[]);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration)
    {
        var schema = new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["type"] = new JsonArray("array", "null"),
            ["items"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["name"] = new JsonObject
                    {
                        ["type"] = new JsonArray("string", "null"),
                        ["description"] = "Display name of the link",
                    },
                    ["target"] = new JsonObject
                    {
                        ["type"] = new JsonArray("string", "null"),
                        ["description"] = "Target attribute (e.g., '_blank')",
                    },
                    ["type"] = new JsonObject
                    {
                        ["type"] = new JsonArray("string", "null"),
                        ["enum"] = new JsonArray("document", "media", "external", null),
                        ["description"] = "Link type (document, media, or external)",
                    },
                    ["unique"] = new JsonObject
                    {
                        ["type"] = new JsonArray("string", "null"),
                        ["format"] = "uuid",
                        ["pattern"] = ValueSchemaPatterns.Uuid,
                        ["description"] = "GUID of linked content/media (for document/media types)",
                    },
                    ["url"] = new JsonObject
                    {
                        ["type"] = new JsonArray("string", "null"),
                        ["description"] = "URL (for external links)",
                    },
                    ["queryString"] = new JsonObject
                    {
                        ["type"] = new JsonArray("string", "null"),
                        ["description"] = "Query string portion of the URL",
                    },
                },
            },
            ["description"] = "Array of link objects",
        };

        // Add minItems/maxItems from configuration if available
        if (configuration is MultiUrlPickerConfiguration pickerConfig)
        {
            if (pickerConfig.MinNumber > 0)
            {
                schema["minItems"] = pickerConfig.MinNumber;
            }

            if (pickerConfig.MaxNumber > 0)
            {
                schema["maxItems"] = pickerConfig.MaxNumber;
            }
        }

        return schema;
    }

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MultiUrlPickerConfigurationEditor(_ioHelper);

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultiUrlPickerValueEditor>(Attribute!);
}
