// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Infrastructure.PropertyEditors;

/// <summary>
/// Helper class for building JSON schemas for block-based property editors.
/// </summary>
internal static class BlockJsonSchemaHelper
{
    /// <summary>
    /// Creates the base block item data schema used by all block editors.
    /// Contains key, contentTypeKey, and values array structure.
    /// </summary>
    /// <returns>A JsonObject representing the block item data schema.</returns>
    public static JsonObject CreateBlockItemDataSchema() =>
        new()
        {
            ["type"] = "object",
            ["required"] = new JsonArray("key", "contentTypeKey"),
            ["properties"] = new JsonObject
            {
                ["key"] = new JsonObject
                {
                    ["type"] = "string",
                    ["format"] = "uuid",
                    ["pattern"] = ValueSchemaPatterns.Uuid,
                },
                ["contentTypeKey"] = new JsonObject
                {
                    ["type"] = "string",
                    ["format"] = "uuid",
                    ["pattern"] = ValueSchemaPatterns.Uuid,
                },
                ["values"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JsonObject
                        {
                            ["alias"] = new JsonObject { ["type"] = "string" },
                            ["culture"] = new JsonObject { ["type"] = new JsonArray("string", "null") },
                            ["segment"] = new JsonObject { ["type"] = new JsonArray("string", "null") },
                            ["value"] = new JsonObject { }, // Any type - depends on property editor
                        },
                    },
                },
            },
        };

    /// <summary>
    /// Creates a content data schema with allowed content type constraints from block configuration.
    /// </summary>
    /// <param name="blocks">The block configurations containing content element type keys.</param>
    /// <returns>A JsonObject representing the content data item schema with type constraints.</returns>
    public static JsonObject CreateContentDataSchema(ICollection<IBlockConfiguration>? blocks)
    {
        JsonObject schema = CreateBlockItemDataSchema();

        if (blocks is null || blocks.Count == 0)
        {
            return schema;
        }

        var allowedContentTypes = new JsonArray();
        foreach (IBlockConfiguration block in blocks)
        {
            allowedContentTypes.Add(JsonValue.Create(block.ContentElementTypeKey.ToString()));
        }

        schema["properties"]!["contentTypeKey"]!.AsObject()["enum"] = allowedContentTypes;
        return schema;
    }

    /// <summary>
    /// Creates a settings data schema with allowed settings type constraints from block configuration.
    /// </summary>
    /// <param name="blocks">The block configurations containing settings element type keys.</param>
    /// <returns>A JsonObject representing the settings data item schema with type constraints.</returns>
    public static JsonObject CreateSettingsDataSchema(ICollection<IBlockConfiguration>? blocks)
    {
        JsonObject schema = CreateBlockItemDataSchema();

        if (blocks is null || blocks.Count == 0)
        {
            return schema;
        }

        var allowedSettingsTypes = new JsonArray();
        foreach (IBlockConfiguration block in blocks)
        {
            if (block.SettingsElementTypeKey.HasValue)
            {
                allowedSettingsTypes.Add(JsonValue.Create(block.SettingsElementTypeKey.Value.ToString()));
            }
        }

        if (allowedSettingsTypes.Count > 0)
        {
            schema["properties"]!["contentTypeKey"]!.AsObject()["enum"] = allowedSettingsTypes;
        }

        return schema;
    }

    /// <summary>
    /// Creates the base layout item schema with contentKey and settingsKey.
    /// Used as the foundation for all block layout items.
    /// </summary>
    /// <returns>A JsonObject representing the base layout item schema.</returns>
    public static JsonObject CreateBaseLayoutItemSchema() =>
        new()
        {
            ["type"] = "object",
            ["required"] = new JsonArray("contentKey"),
            ["properties"] = new JsonObject
            {
                ["contentKey"] = new JsonObject
                {
                    ["type"] = "string",
                    ["format"] = "uuid",
                    ["pattern"] = ValueSchemaPatterns.Uuid,
                },
                ["settingsKey"] = new JsonObject
                {
                    ["type"] = new JsonArray("string", "null"),
                    ["format"] = "uuid",
                    ["pattern"] = ValueSchemaPatterns.Uuid,
                },
            },
        };

    /// <summary>
    /// Creates the expose item schema for block variation.
    /// </summary>
    /// <returns>A JsonObject representing the expose item schema.</returns>
    public static JsonObject CreateExposeItemSchema() =>
        new()
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["contentKey"] = new JsonObject
                {
                    ["type"] = "string",
                    ["format"] = "uuid",
                    ["pattern"] = ValueSchemaPatterns.Uuid,
                },
                ["culture"] = new JsonObject { ["type"] = new JsonArray("string", "null") },
                ["segment"] = new JsonObject { ["type"] = new JsonArray("string", "null") },
            },
        };

    /// <summary>
    /// Applies minItems/maxItems validation constraints to a layout array schema.
    /// </summary>
    /// <param name="layoutArraySchema">The layout array schema to modify.</param>
    /// <param name="min">Optional minimum number of items.</param>
    /// <param name="max">Optional maximum number of items.</param>
    public static void ApplyValidationConstraints(JsonObject layoutArraySchema, int? min, int? max)
    {
        if (min is int minValue && minValue > 0)
        {
            layoutArraySchema["minItems"] = minValue;
        }

        if (max is int maxValue && maxValue > 0)
        {
            layoutArraySchema["maxItems"] = maxValue;
        }
    }

    /// <summary>
    /// Creates the root schema wrapper with $schema and nullable object type.
    /// </summary>
    /// <returns>A JsonObject with the base schema structure.</returns>
    public static JsonObject CreateRootSchema() =>
        new()
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["type"] = new JsonArray("object", "null"),
        };
}
