// Copyright (c) Umbraco.
// See LICENSE for more details.

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
using Umbraco.Extensions;
using BlockGridAreaConfiguration = Umbraco.Cms.Core.PropertyEditors.BlockGridConfiguration.BlockGridAreaConfiguration;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Abstract base class for block grid based editors.
/// </summary>
public abstract class BlockGridPropertyEditorBase : DataEditor
{
    private readonly IBlockValuePropertyIndexValueFactory _blockValuePropertyIndexValueFactory;

    protected BlockGridPropertyEditorBase(IDataValueEditorFactory dataValueEditorFactory, IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory)
        : base(dataValueEditorFactory)
    {
        _blockValuePropertyIndexValueFactory = blockValuePropertyIndexValueFactory;
        SupportsReadOnly = true;
    }

    /// <summary>
    /// Gets the <see cref="IPropertyIndexValueFactory"/> instance used to generate index values for block grid properties.
    /// </summary>
    public override IPropertyIndexValueFactory PropertyIndexValueFactory => _blockValuePropertyIndexValueFactory;


    #region Value Editor

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<BlockGridEditorPropertyValueEditor>(Attribute!);

    internal sealed class BlockGridEditorPropertyValueEditor : BlockEditorPropertyValueEditor<BlockGridValue, BlockGridLayoutItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockGridEditorPropertyValueEditor"/> class.
        /// </summary>
        /// <param name="attribute">The <see cref="DataEditorAttribute"/> that describes the data editor.</param>
        /// <param name="propertyEditors">A collection of available <see cref="PropertyEditorCollection"/> instances.</param>
        /// <param name="dataValueReferenceFactories">A collection of <see cref="DataValueReferenceFactoryCollection"/> used for resolving data value references.</param>
        /// <param name="dataTypeConfigurationCache">The cache for data type configurations.</param>
        /// <param name="textService">The <see cref="ILocalizedTextService"/> for retrieving localized text.</param>
        /// <param name="logger">The <see cref="ILogger{BlockGridEditorPropertyValueEditor}"/> instance for logging.</param>
        /// <param name="shortStringHelper">The <see cref="IShortStringHelper"/> used for string manipulation and formatting.</param>
        /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/> for serializing and deserializing JSON data.</param>
        /// <param name="elementTypeCache">The <see cref="IBlockEditorElementTypeCache"/> for caching block editor element types.</param>
        /// <param name="propertyValidationService">The <see cref="IPropertyValidationService"/> used for validating property values.</param>
        /// <param name="blockEditorVarianceHandler">The <see cref="BlockEditorVarianceHandler"/> for handling block editor variance logic.</param>
        /// <param name="languageService">The <see cref="ILanguageService"/> for managing languages.</param>
        /// <param name="ioHelper">The <see cref="IIOHelper"/> for IO-related helper methods.</param>
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

            /// <summary>
            /// Initializes a new instance of the <see cref="MinMaxValidator"/> class, used to validate minimum and maximum constraints for block grid editor values.
            /// </summary>
            /// <param name="blockEditorValues">The block editor values that will be validated for min/max constraints.</param>
            /// <param name="textService">The service used to provide localized error messages.</param>
            public MinMaxValidator(BlockEditorValues<BlockGridValue, BlockGridLayoutItem> blockEditorValues, ILocalizedTextService textService)
                : base(textService) =>
                _blockEditorValues = blockEditorValues;

            /// <summary>
            /// Validates a block grid editor value against the minimum and maximum block limits specified in the data type configuration.
            /// This includes validating the total number of blocks as well as the allowed number of items within each defined area.
            /// </summary>
            /// <param name="value">The value to validate, typically a serialized block grid editor data structure.</param>
            /// <param name="valueType">The type of the value being validated (may be null or unused).</param>
            /// <param name="dataTypeConfiguration">The configuration object for the data type, expected to be a <see cref="BlockGridConfiguration"/>.</param>
            /// <param name="validationContext">The context for property validation, providing additional validation information.</param>
            /// <returns>
            /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found, or an empty collection if the value is valid.
            /// </returns>
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

        /// <summary>
        /// Retrieves all element type keys configured within the block grid configuration.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{Guid}"/> containing the GUIDs of the configured element types.</returns>
        public override IEnumerable<Guid> ConfiguredElementTypeKeys()
        {
            var configuration = ConfigurationObject as BlockGridConfiguration;
            return configuration?.Blocks.SelectMany(ConfiguredElementTypeKeys) ?? Enumerable.Empty<Guid>();
        }
    }

    #endregion
}
