// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Provides validation logic for block editor property values and their layouts in Umbraco.
/// </summary>
/// <typeparam name="TValue">The type representing the value of the block editor.</typeparam>
/// <typeparam name="TLayout">The type representing the layout structure of the block editor.</typeparam>
public class BlockEditorValidator<TValue, TLayout> : BlockEditorValidatorBase<TValue, TLayout>
    where TValue : BlockValue<TLayout>, new()
    where TLayout : class, IBlockLayoutItem, new()
{
    private readonly BlockEditorValues<TValue, TLayout> _blockEditorValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.PropertyEditors.BlockEditorValidator{TValue, TLayout}"/> class.
    /// </summary>
    /// <param name="propertyValidationService">The service used to validate property values within the block editor.</param>
    /// <param name="blockEditorValues">The values representing the block editor's data and layout.</param>
    /// <param name="elementTypeCache">The cache for block editor element types to optimize lookups.</param>
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
