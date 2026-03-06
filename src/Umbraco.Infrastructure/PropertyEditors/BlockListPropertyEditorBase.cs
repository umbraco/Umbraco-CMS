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
using Umbraco.Cms.Infrastructure.PropertyEditors;
using Umbraco.Cms.Infrastructure.PropertyEditors.Validators;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Abstract base class for block list based editors.
/// </summary>
public abstract class BlockListPropertyEditorBase : DataEditor, IValueSchemaProvider
{
    private readonly IBlockValuePropertyIndexValueFactory _blockValuePropertyIndexValueFactory;
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockListPropertyEditorBase"/> class.
    /// </summary>
    protected BlockListPropertyEditorBase(IDataValueEditorFactory dataValueEditorFactory, IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory, IJsonSerializer jsonSerializer)
        : base(dataValueEditorFactory)
    {
        _blockValuePropertyIndexValueFactory = blockValuePropertyIndexValueFactory;
        _jsonSerializer = jsonSerializer;
        SupportsReadOnly = true;
    }

    /// <inheritdoc/>
    public override IPropertyIndexValueFactory PropertyIndexValueFactory => _blockValuePropertyIndexValueFactory;

    /// <inheritdoc />
    public virtual Type? GetValueType(object? configuration) => typeof(string); // JSON string representation

    /// <inheritdoc />
    public virtual JsonObject? GetValueSchema(object? configuration)
    {
        var config = configuration as BlockListConfiguration;

        // Build layout item schema
        JsonObject layoutItemSchema = BlockJsonSchemaHelper.CreateBaseLayoutItemSchema();

        // Build content and settings data schemas with constraints
        JsonObject contentDataItemSchema = BlockJsonSchemaHelper.CreateContentDataSchema(config?.Blocks);
        JsonObject settingsDataItemSchema = BlockJsonSchemaHelper.CreateSettingsDataSchema(config?.Blocks);

        // Build expose schema (BlockList-specific)
        JsonObject exposeItemSchema = BlockJsonSchemaHelper.CreateExposeItemSchema();

        // Build the main schema
        JsonObject schema = BlockJsonSchemaHelper.CreateRootSchema();
        schema["properties"] = new JsonObject
        {
            ["layout"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    [Constants.PropertyEditors.Aliases.BlockList] = new JsonObject
                    {
                        ["type"] = "array",
                        ["items"] = layoutItemSchema,
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
            ["expose"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = exposeItemSchema,
            },
        };

        // Add validation constraints
        var layoutArray = schema["properties"]!["layout"]!["properties"]![Constants.PropertyEditors.Aliases.BlockList]!.AsObject();
        BlockJsonSchemaHelper.ApplyValidationConstraints(layoutArray, config?.ValidationLimit.Min, config?.ValidationLimit.Max);

        return schema;
    }

    /// <summary>
    /// Instantiates a new <see cref="BlockEditorDataConverter{BlockListValue, BlockListLayoutItem}"/> for use with the block list editor property value editor.
    /// </summary>
    /// <returns>A new instance of <see cref="BlockListEditorDataConverter"/>.</returns>
    protected virtual BlockEditorDataConverter<BlockListValue, BlockListLayoutItem> CreateBlockEditorDataConverter() => new BlockListEditorDataConverter(_jsonSerializer);

    /// <inheritdoc/>
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<BlockListEditorPropertyValueEditor>(Attribute!, CreateBlockEditorDataConverter());

    /// <summary>
    /// Defines the value editor for the block list property editors.
    /// </summary>
    internal sealed class BlockListEditorPropertyValueEditor : BlockEditorPropertyValueEditor<BlockListValue, BlockListLayoutItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockListEditorPropertyValueEditor"/> class.
        /// </summary>
        public BlockListEditorPropertyValueEditor(
            DataEditorAttribute attribute,
            BlockEditorDataConverter<BlockListValue, BlockListLayoutItem> blockEditorDataConverter,
            PropertyEditorCollection propertyEditors,
            DataValueReferenceFactoryCollection dataValueReferenceFactories,
            IDataTypeConfigurationCache dataTypeConfigurationCache,
            IBlockEditorElementTypeCache elementTypeCache,
            ILocalizedTextService textService,
            ILogger<BlockListEditorPropertyValueEditor> logger,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IPropertyValidationService propertyValidationService,
            BlockEditorVarianceHandler blockEditorVarianceHandler,
            ILanguageService languageService,
            IIOHelper ioHelper)
            : base(propertyEditors, dataValueReferenceFactories, dataTypeConfigurationCache, shortStringHelper, jsonSerializer, blockEditorVarianceHandler, languageService, ioHelper, attribute, logger)
        {
            BlockEditorValues = new BlockEditorValues<BlockListValue, BlockListLayoutItem>(blockEditorDataConverter, elementTypeCache, logger);
            Validators.Add(new BlockEditorValidator<BlockListValue, BlockListLayoutItem>(propertyValidationService, BlockEditorValues, elementTypeCache));
            Validators.Add(new MinMaxValidator(BlockEditorValues, textService));
        }

        /// <inheritdoc />
        public override IValueRequiredValidator RequiredValidator => new BlockListValueRequiredValidator(JsonSerializer);

        /// <inheritdoc/>
        protected override BlockListValue CreateWithLayout(IEnumerable<BlockListLayoutItem> layout) => new(layout);

        /// <summary>
        /// Validates the min/max configuration for block list property editors.
        /// </summary>
        private sealed class MinMaxValidator : BlockEditorMinMaxValidatorBase<BlockListValue, BlockListLayoutItem>
        {
            private readonly BlockEditorValues<BlockListValue, BlockListLayoutItem> _blockEditorValues;

            public MinMaxValidator(BlockEditorValues<BlockListValue, BlockListLayoutItem> blockEditorValues, ILocalizedTextService textService)
                : base(textService) =>
                _blockEditorValues = blockEditorValues;

            public override IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
            {
                var blockConfig = (BlockListConfiguration?)dataTypeConfiguration;

                BlockListConfiguration.NumberRange? validationLimit = blockConfig?.ValidationLimit;
                if (validationLimit == null)
                {
                    return Array.Empty<ValidationResult>();
                }

                BlockEditorData<BlockListValue, BlockListLayoutItem>? blockEditorData = _blockEditorValues.DeserializeAndClean(value);

                return ValidateNumberOfBlocks(blockEditorData, validationLimit.Min, validationLimit.Max);
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<Guid> ConfiguredElementTypeKeys()
        {
            var configuration = ConfigurationObject as BlockListConfiguration;
            return configuration?.Blocks.SelectMany(ConfiguredElementTypeKeys) ?? Enumerable.Empty<Guid>();
        }
    }
}
