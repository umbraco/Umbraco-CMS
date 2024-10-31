// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Validates the min/max number of items of a block based editor
/// </summary>
internal abstract class BlockEditorMinMaxValidatorBase<TValue, TLayout> : IValueValidator
    where TValue : BlockValue<TLayout>, new()
    where TLayout : class, IBlockLayoutItem, new()
{
    protected BlockEditorMinMaxValidatorBase(ILocalizedTextService textService) => TextService = textService;

    protected ILocalizedTextService TextService { get; }

    public abstract IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext);

    protected IEnumerable<ValidationResult> ValidateNumberOfBlocks(BlockEditorData<TValue, TLayout>? blockEditorData, int? min, int? max)
    {
        var numberOfBlocks = blockEditorData?.Layout?.Count() ?? 0;

        if (min.HasValue)
        {
            if ((blockEditorData == null && min > 0)
                || (blockEditorData != null && numberOfBlocks < min))
            {
                yield return new ValidationResult(
                    TextService.Localize(
                        "validation",
                        "entriesShort",
                        new[] { min.ToString(), (min - numberOfBlocks).ToString(), }),
                    new[] { "minCount" });
            }
        }

        if (blockEditorData != null && max.HasValue && numberOfBlocks > max)
        {
            yield return new ValidationResult(
                TextService.Localize(
                    "validation",
                    "entriesExceed",
                    new[] { max.ToString(), (numberOfBlocks - max).ToString(), }),
                new[] { "maxCount" });
        }
    }
}
