using System.ComponentModel.DataAnnotations;
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
using Umbraco.Cms.Infrastructure.PropertyEditors.Validators;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Abstract base class for block list based editors.
/// </summary>
public abstract class BlockListPropertyEditorBase : DataEditor
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
    internal class BlockListEditorPropertyValueEditor : BlockEditorPropertyValueEditor<BlockListValue, BlockListLayoutItem>
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
            : base(propertyEditors, dataValueReferenceFactories, dataTypeConfigurationCache, shortStringHelper, jsonSerializer, blockEditorVarianceHandler, languageService, ioHelper, attribute)
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
        private class MinMaxValidator : BlockEditorMinMaxValidatorBase<BlockListValue, BlockListLayoutItem>
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
