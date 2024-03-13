// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class BlockEditorValidator<TValue, TLayout> : BlockEditorValidatorBase<TValue, TLayout>
    where TValue : BlockValue<TLayout>, new()
    where TLayout : class, IBlockLayoutItem, new()
{
    private readonly BlockEditorValues<TValue, TLayout> _blockEditorValues;

    public BlockEditorValidator(
        IPropertyValidationService propertyValidationService,
        BlockEditorValues<TValue, TLayout> blockEditorValues,
        IContentTypeService contentTypeService)
        : base(propertyValidationService, contentTypeService)
        => _blockEditorValues = blockEditorValues;

    protected override IEnumerable<ElementTypeValidationModel> GetElementTypeValidation(object? value)
    {
        BlockEditorData<TValue, TLayout>? blockEditorData = _blockEditorValues.DeserializeAndClean(value);
        return blockEditorData is not null
            ? GetBlockEditorDataValidation(blockEditorData)
            : Array.Empty<ElementTypeValidationModel>();
    }
}
