// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a property editor in Umbraco that enables users to select and manage URLs within a property.
/// </summary>
/// <remarks>
/// There is one URL picker editor per number of links a picker holds - a single link or several - so that the type
/// a URL picker property yields follows from the editor rather than from the configuration of the data type it is
/// used through. Both are edited and stored the same way, which is what this base holds.
/// </remarks>
public abstract class MultiUrlPickerPropertyEditorBase : DataEditor, IValueSchemaProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiUrlPickerPropertyEditorBase"/> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">Factory for creating data value editors.</param>
    protected MultiUrlPickerPropertyEditorBase(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;

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

        if (configuration is SingleUrlPickerConfiguration)
        {
            schema["maxItems"] = 1;
        }

        return schema;
    }

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultiUrlPickerValueEditor>(Attribute!);
}
