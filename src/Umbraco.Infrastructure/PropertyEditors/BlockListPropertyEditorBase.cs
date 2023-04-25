using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Abstract base class for block list based editors.
/// </summary>
public abstract class BlockListPropertyEditorBase : DataEditor
{

    private readonly IBlockValuePropertyIndexValueFactory _blockValuePropertyIndexValueFactory;

    [Obsolete("Use non-obsoleted ctor. This will be removed in Umbraco 13.")]
    protected BlockListPropertyEditorBase(IDataValueEditorFactory dataValueEditorFactory)
        : this(dataValueEditorFactory, StaticServiceProvider.Instance.GetRequiredService<IBlockValuePropertyIndexValueFactory>())
    {

    }

    protected BlockListPropertyEditorBase(IDataValueEditorFactory dataValueEditorFactory, IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory)
        : base(dataValueEditorFactory)
    {
        _blockValuePropertyIndexValueFactory = blockValuePropertyIndexValueFactory;
        SupportsReadOnly = true;
    }

    public override IPropertyIndexValueFactory PropertyIndexValueFactory => _blockValuePropertyIndexValueFactory;


    #region Value Editor

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<BlockListEditorPropertyValueEditor>(Attribute!);

    internal class BlockListEditorPropertyValueEditor : BlockEditorPropertyValueEditor<BlockListValue, BlockListLayoutItem>
    {
        public BlockListEditorPropertyValueEditor(
            DataEditorAttribute attribute,
            PropertyEditorCollection propertyEditors,
            IDataTypeService dataTypeService,
            IContentTypeService contentTypeService,
            ILocalizedTextService textService,
            ILogger<BlockListEditorPropertyValueEditor> logger,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            IPropertyValidationService propertyValidationService) :
            base(attribute, propertyEditors, dataTypeService, textService, logger, shortStringHelper, jsonSerializer, ioHelper)
        {
            BlockEditorValues = new BlockEditorValues<BlockListValue, BlockListLayoutItem>(new BlockListEditorDataConverter(jsonSerializer), contentTypeService, logger);
            Validators.Add(new BlockEditorValidator<BlockListValue, BlockListLayoutItem>(propertyValidationService, BlockEditorValues, contentTypeService));
            Validators.Add(new MinMaxValidator(BlockEditorValues, textService));
        }

        private class MinMaxValidator : BlockEditorMinMaxValidatorBase<BlockListValue, BlockListLayoutItem>
        {
            private readonly BlockEditorValues<BlockListValue, BlockListLayoutItem> _blockEditorValues;

            public MinMaxValidator(BlockEditorValues<BlockListValue, BlockListLayoutItem> blockEditorValues, ILocalizedTextService textService)
                : base(textService) =>
                _blockEditorValues = blockEditorValues;

            public override IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration)
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
    }

    #endregion
}
