// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using BlockGridAreaConfiguration = Umbraco.Cms.Core.PropertyEditors.BlockGridConfiguration.BlockGridAreaConfiguration;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Abstract base class for block grid based editors.
/// </summary>
public abstract class BlockGridPropertyEditorBase : DataEditor, IValueSchemaProvider
{
    private readonly IBlockValuePropertyIndexValueFactory _blockValuePropertyIndexValueFactory;

    protected BlockGridPropertyEditorBase(IDataValueEditorFactory dataValueEditorFactory, IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory)
        : base(dataValueEditorFactory)
    {
        _blockValuePropertyIndexValueFactory = blockValuePropertyIndexValueFactory;
        SupportsReadOnly = true;
    }

    public override IPropertyIndexValueFactory PropertyIndexValueFactory => _blockValuePropertyIndexValueFactory;

    /// <inheritdoc />
    public virtual Type? GetValueType(object? configuration) => typeof(string); // JSON string representation

    /// <inheritdoc />
    public virtual JsonObject? GetValueSchema(object? configuration)
    {
        var config = configuration as BlockGridConfiguration;

        // Build area item schema
        var areaItemSchema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["key"] = new JsonObject { ["type"] = "string", ["format"] = "uuid", ["pattern"] = ValueSchemaPatterns.Uuid },
                ["items"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject { ["$ref"] = "#/$defs/layoutItem" },
                },
            },
        };

        // Build layout item schema (with grid-specific properties)
        var layoutItemSchema = new JsonObject
        {
            ["type"] = "object",
            ["required"] = new JsonArray("contentKey"),
            ["properties"] = new JsonObject
            {
                ["contentKey"] = new JsonObject { ["type"] = "string", ["format"] = "uuid", ["pattern"] = ValueSchemaPatterns.Uuid },
                ["settingsKey"] = new JsonObject { ["type"] = new JsonArray("string", "null"), ["format"] = "uuid", ["pattern"] = ValueSchemaPatterns.Uuid },
                ["columnSpan"] = new JsonObject { ["type"] = "integer", ["minimum"] = 1 },
                ["rowSpan"] = new JsonObject { ["type"] = "integer", ["minimum"] = 1 },
                ["areas"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = areaItemSchema,
                },
            },
        };

        // Build block item data schema
        var blockItemDataSchema = new JsonObject
        {
            ["type"] = "object",
            ["required"] = new JsonArray("key", "contentTypeKey"),
            ["properties"] = new JsonObject
            {
                ["key"] = new JsonObject { ["type"] = "string", ["format"] = "uuid", ["pattern"] = ValueSchemaPatterns.Uuid },
                ["contentTypeKey"] = new JsonObject { ["type"] = "string", ["format"] = "uuid", ["pattern"] = ValueSchemaPatterns.Uuid },
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

        // Build content data schema with allowed content type constraints
        var contentDataItemSchema = blockItemDataSchema.DeepClone().AsObject();
        if (config?.Blocks is { Length: > 0 })
        {
            var allowedContentTypes = new JsonArray();
            foreach (var block in config.Blocks)
            {
                allowedContentTypes.Add(JsonValue.Create(block.ContentElementTypeKey.ToString()));
            }

            contentDataItemSchema["properties"]!["contentTypeKey"]!.AsObject()["enum"] = allowedContentTypes;
        }

        // Build settings data schema with allowed settings type constraints
        var settingsDataItemSchema = blockItemDataSchema.DeepClone().AsObject();
        if (config?.Blocks is { Length: > 0 })
        {
            var allowedSettingsTypes = new JsonArray();
            foreach (var block in config.Blocks)
            {
                if (block.SettingsElementTypeKey.HasValue)
                {
                    allowedSettingsTypes.Add(JsonValue.Create(block.SettingsElementTypeKey.Value.ToString()));
                }
            }

            if (allowedSettingsTypes.Count > 0)
            {
                settingsDataItemSchema["properties"]!["contentTypeKey"]!.AsObject()["enum"] = allowedSettingsTypes;
            }
        }

        // Build the main schema
        var schema = new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["type"] = new JsonArray("object", "null"),
            ["$defs"] = new JsonObject
            {
                ["layoutItem"] = layoutItemSchema,
            },
            ["properties"] = new JsonObject
            {
                ["layout"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        [Constants.PropertyEditors.Aliases.BlockGrid] = new JsonObject
                        {
                            ["type"] = "array",
                            ["items"] = new JsonObject { ["$ref"] = "#/$defs/layoutItem" },
                        },
                    },
                },
                ["contentData"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = contentDataItemSchema,
                },
                ["settingsData"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = settingsDataItemSchema,
                },
            },
        };

        // Add grid columns constraint from configuration
        if (config?.GridColumns is int gridColumns && gridColumns > 0)
        {
            layoutItemSchema["properties"]!["columnSpan"]!.AsObject()["maximum"] = gridColumns;
        }

        // Add validation constraints
        if (config?.ValidationLimit?.Min is int min && min > 0)
        {
            var layoutArray = schema["properties"]!["layout"]!["properties"]![Constants.PropertyEditors.Aliases.BlockGrid]!.AsObject();
            layoutArray["minItems"] = min;
        }

        if (config?.ValidationLimit?.Max is int max && max > 0)
        {
            var layoutArray = schema["properties"]!["layout"]!["properties"]![Constants.PropertyEditors.Aliases.BlockGrid]!.AsObject();
            layoutArray["maxItems"] = max;
        }

        return schema;
    }

    #region Value Editor

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<BlockGridEditorPropertyValueEditor>(Attribute!);

    internal sealed class BlockGridEditorPropertyValueEditor : BlockEditorPropertyValueEditor<BlockGridValue, BlockGridLayoutItem>
    {
        public BlockGridEditorPropertyValueEditor(
            DataEditorAttribute attribute,
            PropertyEditorCollection propertyEditors,
            DataValueReferenceFactoryCollection dataValueReferenceFactories,
            IDataTypeConfigurationCache dataTypeConfigurationCache,
            ILocalizedTextService textService,
            ILogger<BlockGridEditorPropertyValueEditor> logger,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IBlockEditorElementTypeCache elementTypeCache,
            IPropertyValidationService propertyValidationService,
            BlockEditorVarianceHandler blockEditorVarianceHandler,
            ILanguageService languageService,
            IIOHelper ioHelper)
            : base(propertyEditors, dataValueReferenceFactories, dataTypeConfigurationCache, shortStringHelper, jsonSerializer, blockEditorVarianceHandler, languageService, ioHelper, attribute, logger)
        {
            BlockEditorValues = new BlockEditorValues<BlockGridValue, BlockGridLayoutItem>(new BlockGridEditorDataConverter(jsonSerializer), elementTypeCache, logger);
            Validators.Add(new BlockEditorValidator<BlockGridValue, BlockGridLayoutItem>(propertyValidationService, BlockEditorValues, elementTypeCache));
            Validators.Add(new MinMaxValidator(BlockEditorValues, textService));
        }

        protected override BlockGridValue CreateWithLayout(IEnumerable<BlockGridLayoutItem> layout) => new(layout);

        private sealed class MinMaxValidator : BlockEditorMinMaxValidatorBase<BlockGridValue, BlockGridLayoutItem>
        {
            private readonly BlockEditorValues<BlockGridValue, BlockGridLayoutItem> _blockEditorValues;

            public MinMaxValidator(BlockEditorValues<BlockGridValue, BlockGridLayoutItem> blockEditorValues, ILocalizedTextService textService)
                : base(textService) =>
                _blockEditorValues = blockEditorValues;

            public override IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
            {
                if (dataTypeConfiguration is not BlockGridConfiguration blockConfig)
                {
                    return Array.Empty<ValidationResult>();
                }

                BlockEditorData<BlockGridValue, BlockGridLayoutItem>? blockEditorData = _blockEditorValues.DeserializeAndClean(value);

                var validationResults = new List<ValidationResult>();
                validationResults.AddRange(ValidateNumberOfBlocks(blockEditorData, blockConfig.ValidationLimit.Min, blockConfig.ValidationLimit.Max));

                var areasConfigsByKey = blockConfig.Blocks.SelectMany(b => b.Areas).ToDictionary(a => a.Key);

                IList<BlockGridLayoutAreaItem> ExtractLayoutAreaItems(BlockGridLayoutItem item)
                {
                    var areas = item.Areas.ToList();
                    areas.AddRange(item.Areas.SelectMany(a => a.Items).SelectMany(ExtractLayoutAreaItems));
                    return areas;
                }

                BlockGridLayoutAreaItem[]? areas = blockEditorData?.Layout?.SelectMany(ExtractLayoutAreaItems).ToArray();

                if (areas?.Any() != true)
                {
                    return validationResults;
                }

                foreach (BlockGridLayoutAreaItem area in areas)
                {
                    if (!areasConfigsByKey.TryGetValue(area.Key, out BlockGridAreaConfiguration? areaConfig))
                    {
                        continue;
                    }

                    if ((areaConfig.MinAllowed.HasValue && area.Items.Length < areaConfig.MinAllowed) || (areaConfig.MaxAllowed.HasValue && area.Items.Length > areaConfig.MaxAllowed))
                    {
                        validationResults.Add(new ValidationResult(TextService.Localize("validation", "entriesAreasMismatch")));
                    }
                }

                return validationResults;
            }
        }

        public override IEnumerable<Guid> ConfiguredElementTypeKeys()
        {
            var configuration = ConfigurationObject as BlockGridConfiguration;
            return configuration?.Blocks.SelectMany(ConfiguredElementTypeKeys) ?? Enumerable.Empty<Guid>();
        }
    }

    #endregion
}
