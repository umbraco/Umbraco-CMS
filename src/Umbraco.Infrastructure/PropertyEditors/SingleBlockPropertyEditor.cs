using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.PropertyEditors;

/// <summary>
/// Represents a single block property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.SingleBlock,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = false)]
public class SingleBlockPropertyEditor : DataEditor
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IIOHelper _ioHelper;
    private readonly IBlockValuePropertyIndexValueFactory _blockValuePropertyIndexValueFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleBlockPropertyEditor"/> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">A factory used to create data value editors for property editing.</param>
    /// <param name="jsonSerializer">The JSON serializer used for serializing and deserializing property values.</param>
    /// <param name="ioHelper">An IO helper for file and path operations.</param>
    /// <param name="blockValuePropertyIndexValueFactory">A factory for creating index values for block value properties.</param>
    public SingleBlockPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory)
        : base(dataValueEditorFactory)
    {
        _jsonSerializer = jsonSerializer;
        _ioHelper = ioHelper;
        _blockValuePropertyIndexValueFactory = blockValuePropertyIndexValueFactory;
    }

    /// <summary>
    /// Gets the <see cref="IPropertyIndexValueFactory"/> instance used to generate index values for properties in the single block property editor.
    /// </summary>
    public override IPropertyIndexValueFactory PropertyIndexValueFactory => _blockValuePropertyIndexValueFactory;

    /// <inheritdoc/>
    public override bool SupportsConfigurableElements => true;

    /// <summary>
    /// Instantiates a new <see cref="BlockEditorDataConverter{SingleBlockValue, SingleBlockLayoutItem}"/> for use with the single block editor property value editor.
    /// </summary>
    /// <returns>A new instance of <see cref="SingleBlockEditorDataConverter"/>.</returns>
    protected virtual BlockEditorDataConverter<SingleBlockValue, SingleBlockLayoutItem> CreateBlockEditorDataConverter()
        => new SingleBlockEditorDataConverter(_jsonSerializer);

    /// <inheritdoc/>
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<SingleBlockEditorPropertyValueEditor>(Attribute!, CreateBlockEditorDataConverter());

    /// <inheritdoc />
    public override bool CanMergePartialPropertyValues(IPropertyType propertyType) => propertyType.VariesByCulture() is false;

    /// <inheritdoc />
    public override object? MergePartialPropertyValueForCulture(object? sourceValue, object? targetValue, string? culture)
    {
        var valueEditor = (SingleBlockEditorPropertyValueEditor)GetValueEditor();
        return valueEditor.MergePartialPropertyValueForCulture(sourceValue, targetValue, culture);
    }

    /// <inheritdoc/>
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new SingleBlockConfigurationEditor(_ioHelper);

    /// <inheritdoc/>
    public override object? MergeVariantInvariantPropertyValue(
        object? sourceValue,
        object? targetValue,
        bool canUpdateInvariantData,
        HashSet<string> allowedCultures)
    {
        var valueEditor = (SingleBlockEditorPropertyValueEditor)GetValueEditor();
        return valueEditor.MergeVariantInvariantPropertyValue(sourceValue, targetValue, canUpdateInvariantData, allowedCultures);
    }

    internal sealed class SingleBlockEditorPropertyValueEditor : BlockEditorPropertyValueEditor<SingleBlockValue, SingleBlockLayoutItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleBlockEditorPropertyValueEditor"/> class with the specified dependencies.
        /// </summary>
        /// <param name="attribute">The <see cref="DataEditorAttribute"/> that describes the data editor.</param>
        /// <param name="blockEditorDataConverter">The <see cref="BlockEditorDataConverter{SingleBlockValue, SingleBlockLayoutItem}"/> used to convert block editor data.</param>
        /// <param name="propertyEditors">A collection of available <see cref="PropertyEditorCollection"/> instances.</param>
        /// <param name="dataValueReferenceFactories">A collection of <see cref="DataValueReferenceFactoryCollection"/> used for data value references.</param>
        /// <param name="dataTypeConfigurationCache">The <see cref="IDataTypeConfigurationCache"/> for caching data type configurations.</param>
        /// <param name="shortStringHelper">The <see cref="IShortStringHelper"/> for string manipulation and formatting.</param>
        /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/> for serializing and deserializing JSON data.</param>
        /// <param name="blockEditorVarianceHandler">The <see cref="BlockEditorVarianceHandler"/> for handling block editor variance.</param>
        /// <param name="languageService">The <see cref="ILanguageService"/> for language management and localization.</param>
        /// <param name="ioHelper">The <see cref="IIOHelper"/> for IO operations and path handling.</param>
        /// <param name="elementTypeCache">The <see cref="IBlockEditorElementTypeCache"/> for caching block editor element types.</param>
        /// <param name="logger">The <see cref="ILogger{SingleBlockEditorPropertyValueEditor}"/> instance for logging.</param>
        /// <param name="textService">The <see cref="ILocalizedTextService"/> for retrieving localized text.</param>
        /// <param name="propertyValidationService">The <see cref="IPropertyValidationService"/> for property validation logic.</param>
        public SingleBlockEditorPropertyValueEditor(
            DataEditorAttribute attribute,
            BlockEditorDataConverter<SingleBlockValue, SingleBlockLayoutItem> blockEditorDataConverter,
            PropertyEditorCollection propertyEditors,
            DataValueReferenceFactoryCollection dataValueReferenceFactories,
            IDataTypeConfigurationCache dataTypeConfigurationCache,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            BlockEditorVarianceHandler blockEditorVarianceHandler,
            ILanguageService languageService,
            IIOHelper ioHelper,
            IBlockEditorElementTypeCache elementTypeCache,
            ILogger<SingleBlockEditorPropertyValueEditor> logger,
            ILocalizedTextService textService,
            IPropertyValidationService propertyValidationService)
            : base(propertyEditors, dataValueReferenceFactories, dataTypeConfigurationCache, shortStringHelper, jsonSerializer, blockEditorVarianceHandler, languageService, ioHelper, attribute, logger)
        {
            BlockEditorValues = new BlockEditorValues<SingleBlockValue, SingleBlockLayoutItem>(blockEditorDataConverter, elementTypeCache, logger);
            Validators.Add(new BlockEditorValidator<SingleBlockValue, SingleBlockLayoutItem>(propertyValidationService, BlockEditorValues, elementTypeCache));
            Validators.Add(new SingleBlockValidator(BlockEditorValues, textService));
        }

        protected override SingleBlockValue CreateWithLayout(IEnumerable<SingleBlockLayoutItem> layout) =>
            new(layout.Single());

        /// <inheritdoc/>
        public override IEnumerable<Guid> ConfiguredElementTypeKeys()
        {
            var configuration = ConfigurationObject as SingleBlockConfiguration;
            return configuration?.Blocks.SelectMany(ConfiguredElementTypeKeys) ?? Enumerable.Empty<Guid>();
        }

        /// <summary>
        /// Validates whether the block editor holds a single value
        /// </summary>
        internal sealed class SingleBlockValidator : BlockEditorMinMaxValidatorBase<SingleBlockValue, SingleBlockLayoutItem>
        {
            private readonly BlockEditorValues<SingleBlockValue, SingleBlockLayoutItem> _blockEditorValues;

            /// <summary>
            /// Initializes a new instance of the <see cref="SingleBlockValidator"/> class for validating single block editor values.
            /// </summary>
            /// <param name="blockEditorValues">The block editor values to be validated.</param>
            /// <param name="textService">The service used for providing localized text messages during validation.</param>
            public SingleBlockValidator(BlockEditorValues<SingleBlockValue, SingleBlockLayoutItem> blockEditorValues, ILocalizedTextService textService)
                : base(textService) =>
                _blockEditorValues = blockEditorValues;

            /// <summary>
            /// Validates the value for the single block editor, ensuring the correct number of blocks are present.
            /// </summary>
            /// <param name="value">The value to validate, typically the serialized block editor data.</param>
            /// <param name="valueType">The type of the value being validated.</param>
            /// <param name="dataTypeConfiguration">The configuration for the data type.</param>
            /// <param name="validationContext">The context for property validation.</param>
            /// <returns>An enumerable of <see cref="ValidationResult"/> indicating any validation errors related to the number of blocks.</returns>
            public override IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
            {
                BlockEditorData<SingleBlockValue, SingleBlockLayoutItem>? blockEditorData = _blockEditorValues.DeserializeAndClean(value);

                return ValidateNumberOfBlocks(blockEditorData, 0, 1);
            }
        }
    }
}
