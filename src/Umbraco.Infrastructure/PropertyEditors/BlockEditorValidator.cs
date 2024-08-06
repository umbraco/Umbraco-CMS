// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class BlockEditorValidator : BlockEditorValidatorBase
{
    private readonly BlockEditorValues _blockEditorValues;

    public BlockEditorValidator(
        IPropertyValidationService propertyValidationService,
        BlockEditorValues blockEditorValues,
        IBlockEditorElementTypeCache elementTypeCache)
        : base(propertyValidationService, elementTypeCache)
        => _blockEditorValues = blockEditorValues;

    protected override IEnumerable<ElementTypeValidationModel> GetElementTypeValidation(object? value)
    {
        BlockEditorData? blockEditorData = _blockEditorValues.DeserializeAndClean(value);
        return blockEditorData is not null
            ? GetBlockEditorDataValidation(blockEditorData)
            : Array.Empty<ElementTypeValidationModel>();
    }
}
