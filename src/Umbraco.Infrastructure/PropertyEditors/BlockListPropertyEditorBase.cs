using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Abstract base class for block list based editors.
/// </summary>
public abstract class BlockListPropertyEditorBase : DataEditor
{
    protected BlockListPropertyEditorBase(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory) =>
        SupportsReadOnly = true;

    #region Value Editor

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<BlockListEditorPropertyValueEditor>(Attribute!);

    private class BlockListEditorPropertyValueEditor : BlockEditorPropertyValueEditor
    {
        public BlockListEditorPropertyValueEditor(
            DataEditorAttribute attribute,
            PropertyEditorCollection propertyEditors,
            IDataTypeService dataTypeService,
            IContentTypeService contentTypeService,
            ILocalizedTextService textService,
            ILogger<BlockEditorPropertyValueEditor> logger,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            IPropertyValidationService propertyValidationService) :
            base(attribute, propertyEditors, dataTypeService, textService, logger, shortStringHelper, jsonSerializer, ioHelper)
        {
            BlockEditorValues = new BlockEditorValues(new BlockListEditorDataConverter(), contentTypeService, logger);
            Validators.Add(new BlockEditorValidator(propertyValidationService, BlockEditorValues, contentTypeService));
            Validators.Add(new MinMaxValidator(BlockEditorValues, textService));
        }

        /// <summary>
        ///     Validates the min/max of the block editor
        /// </summary>
        private class MinMaxValidator : IValueValidator
        {
            private readonly BlockEditorValues _blockEditorValues;
            private readonly ILocalizedTextService _textService;

            public MinMaxValidator(BlockEditorValues blockEditorValues, ILocalizedTextService textService)
            {
                _blockEditorValues = blockEditorValues;
                _textService = textService;
            }

            public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration)
            {
                var blockConfig = (BlockListConfiguration?)dataTypeConfiguration;
                if (blockConfig == null)
                {
                    yield break;
                }

                BlockListConfiguration.NumberRange? validationLimit = blockConfig.ValidationLimit;
                if (validationLimit == null)
                {
                    yield break;
                }

                BlockEditorData? blockEditorData = _blockEditorValues.DeserializeAndClean(value);

                if ((blockEditorData == null && validationLimit.Min.HasValue && validationLimit.Min > 0)
                    || (blockEditorData != null && validationLimit.Min.HasValue &&
                        blockEditorData.Layout?.Count() < validationLimit.Min))
                {
                    yield return new ValidationResult(
                        _textService.Localize(
                            "validation",
                            "entriesShort",
                            new[]
                            {
                                validationLimit.Min.ToString(),
                                (validationLimit.Min - (blockEditorData?.Layout?.Count() ?? 0)).ToString(),
                            }),
                        new[] { "minCount" });
                }

                if (blockEditorData != null && validationLimit.Max.HasValue &&
                    blockEditorData.Layout?.Count() > validationLimit.Max)
                {
                    yield return new ValidationResult(
                        _textService.Localize(
                            "validation",
                            "entriesExceed",
                            new[]
                            {
                                validationLimit.Max.ToString(),
                                (blockEditorData.Layout.Count() - validationLimit.Max).ToString(),
                            }),
                        new[] { "maxCount" });
                }
            }
        }
    }

    #endregion
}
