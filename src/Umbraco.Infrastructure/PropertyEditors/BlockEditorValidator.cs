// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

public class BlockEditorValidator<TValue, TLayout> : BlockEditorValidatorBase<TValue, TLayout>
    where TValue : BlockValue<TLayout>, new()
    where TLayout : class, IBlockLayoutItem, new()
{
    private readonly BlockEditorValues<TValue, TLayout> _blockEditorValues;

    public BlockEditorValidator(
        IPropertyValidationService propertyValidationService,
        BlockEditorValues<TValue, TLayout> blockEditorValues,
        IBlockEditorElementTypeCache elementTypeCache)
        : base(propertyValidationService, elementTypeCache)
        => _blockEditorValues = blockEditorValues;

    protected override IEnumerable<ElementTypeValidationModel> GetElementTypeValidation(object? value, PropertyValidationContext validationContext)
    {
        BlockEditorData<TValue, TLayout>? blockEditorData = _blockEditorValues.DeserializeAndClean(value);
        return blockEditorData is not null
            ? GetBlockEditorDataValidation(blockEditorData, validationContext)
            : Array.Empty<ElementTypeValidationModel>();
    }
}
