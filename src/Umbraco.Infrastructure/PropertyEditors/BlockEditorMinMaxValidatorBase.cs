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
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockEditorMinMaxValidatorBase{TValue, TLayout}"/> class.
    /// </summary>
    protected BlockEditorMinMaxValidatorBase(ILocalizedTextService textService) => TextService = textService;

    /// <summary>
    /// Gets the <see cref="ILocalizedTextService"/>
    /// </summary>
    protected ILocalizedTextService TextService { get; }

    /// <inheritdoc/>
    public abstract IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext);

    // internal method so we can test for error messages being returned without keeping strings in sync
    internal static string BuildErrorMessage(
        ILocalizedTextService textService,
        int? maxNumberOfBlocks,
        int numberOfBlocks)
        => textService.Localize(
            "validation",
            "entriesExceed",
            [maxNumberOfBlocks.ToString(), (numberOfBlocks - maxNumberOfBlocks).ToString(),]);

    /// <summary>
    /// Validates the number of blocks are within the configured minimum and maximum values.
    /// </summary>
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
                        [min.ToString(), (min - numberOfBlocks).ToString(),]),
                    ["value"]);
            }
        }

        if (blockEditorData != null && max.HasValue && numberOfBlocks > max)
        {
            yield return new ValidationResult(
                BuildErrorMessage(TextService, max, numberOfBlocks),
                ["value"]);
        }
    }
}
