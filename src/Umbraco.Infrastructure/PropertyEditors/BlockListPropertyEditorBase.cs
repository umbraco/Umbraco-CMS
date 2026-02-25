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
    internal sealed class BlockListEditorPropertyValueEditor : BlockEditorPropertyValueEditor<BlockListValue, BlockListLayoutItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockListEditorPropertyValueEditor"/> class.
        /// </summary>
        /// <param name="attribute">The data editor attribute that defines metadata for the property editor.</param>
        /// <param name="blockEditorDataConverter">The converter used for block editor data values and layouts.</param>
        /// <param name="propertyEditors">A collection of available property editors.</param>
        /// <param name="dataValueReferenceFactories">A collection of factories for creating data value references.</param>
        /// <param name="dataTypeConfigurationCache">The cache for data type configurations.</param>
        /// <param name="elementTypeCache">The cache for block editor element types.</param>
        /// <param name="textService">The service for retrieving localized text.</param>
        /// <param name="logger">The logger for diagnostic and error messages.</param>
        /// <param name="shortStringHelper">Helper for generating and manipulating short strings.</param>
        /// <param name="jsonSerializer">The serializer for JSON operations.</param>
        /// <param name="propertyValidationService">Service for validating property values.</param>
        /// <param name="blockEditorVarianceHandler">Handler for managing block editor variance.</param>
        /// <param name="languageService">Service for managing languages.</param>
        /// <param name="ioHelper">Helper for IO operations.</param>
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

            /// <summary>
            /// Initializes a new instance of the <see cref="MinMaxValidator"/> class for validating block editor values against minimum and maximum constraints.
            /// </summary>
            /// <param name="blockEditorValues">The collection of block editor values to be validated.</param>
            /// <param name="textService">The service used to provide localized validation messages.</param>
            public MinMaxValidator(BlockEditorValues<BlockListValue, BlockListLayoutItem> blockEditorValues, ILocalizedTextService textService)
                : base(textService) =>
                _blockEditorValues = blockEditorValues;

            /// <summary>
            /// Validates whether the number of blocks in the provided value falls within the minimum and maximum limits defined in the data type configuration.
            /// Returns validation errors if the number of blocks is outside the allowed range.
            /// </summary>
            /// <param name="value">The value to validate, typically representing the block list data.</param>
            /// <param name="valueType">The type of the value, if applicable.</param>
            /// <param name="dataTypeConfiguration">The configuration object containing validation limits for the block list.</param>
            /// <param name="validationContext">The context for property validation, providing additional information for validation.</param>
            /// <returns>An enumerable of <see cref="ValidationResult"/> objects indicating any validation errors related to the number of blocks.</returns>
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
