// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Abstract base class for block grid based editors.
/// </summary>
public abstract class BlockGridPropertyEditorBase : DataEditor
{
    protected BlockGridPropertyEditorBase(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory) =>
        SupportsReadOnly = true;

    #region Value Editor

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<BlockGridEditorPropertyValueEditor>(Attribute!);

    private class BlockGridEditorPropertyValueEditor : BlockEditorPropertyValueEditor
    {
        public BlockGridEditorPropertyValueEditor(
            DataEditorAttribute attribute,
            PropertyEditorCollection propertyEditors,
            IDataTypeService dataTypeService,
            ILocalizedTextService textService,
            ILogger<BlockEditorPropertyValueEditor> logger,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            IContentTypeService contentTypeService,
            IPropertyValidationService propertyValidationService)
            : base(attribute, propertyEditors, dataTypeService, textService, logger, shortStringHelper, jsonSerializer, ioHelper)
        {
            BlockEditorValues = new BlockEditorValues(new BlockGridEditorDataConverter(jsonSerializer), contentTypeService, logger);
            Validators.Add(new BlockEditorValidator(propertyValidationService, BlockEditorValues, contentTypeService));
            Validators.Add(new MinMaxValidator(BlockEditorValues, textService));
        }


        /// <summary>
        /// Validates the min/max of the block editor
        /// </summary>
        // TODO: implement this 
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
                yield break;
            }
        }
    }

    #endregion
}
