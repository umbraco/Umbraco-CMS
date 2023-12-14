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

    /// <summary>
    /// Instantiates a new <see cref="BlockEditorDataConverter"/> for use with the block list editor property value editor.
    /// </summary>
    /// <returns>A new instance of <see cref="BlockListEditorDataConverter"/>.</returns>
    protected virtual BlockEditorDataConverter CreateBlockEditorDataConverter() => new BlockListEditorDataConverter();

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<BlockListEditorPropertyValueEditor>(Attribute!, CreateBlockEditorDataConverter());

    internal class BlockListEditorPropertyValueEditor : BlockEditorPropertyValueEditor
    {
        public BlockListEditorPropertyValueEditor(
            DataEditorAttribute attribute,
            BlockEditorDataConverter blockEditorDataConverter,
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
            BlockEditorValues = new BlockEditorValues(blockEditorDataConverter, contentTypeService, logger);
            Validators.Add(new BlockEditorValidator(propertyValidationService, BlockEditorValues, contentTypeService));
            Validators.Add(new MinMaxValidator(BlockEditorValues, textService));
        }

        private class MinMaxValidator : BlockEditorMinMaxValidatorBase
        {
            private readonly BlockEditorValues _blockEditorValues;

            public MinMaxValidator(BlockEditorValues blockEditorValues, ILocalizedTextService textService)
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

                BlockEditorData? blockEditorData = _blockEditorValues.DeserializeAndClean(value);

                return ValidateNumberOfBlocks(blockEditorData, validationLimit.Min, validationLimit.Max);
            }
        }
    }

    #endregion
}
