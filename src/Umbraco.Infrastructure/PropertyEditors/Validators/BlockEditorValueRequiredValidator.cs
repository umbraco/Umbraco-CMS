using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.Validators;

/// <summary>
/// Custom validator for block value required validation.
/// </summary>
/// <typeparam name="TValue">The type of block value held by the property editor.</typeparam>
internal sealed class BlockEditorValueRequiredValidator<TValue> : RequiredValidator
    where TValue : BlockValue
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockEditorValueRequiredValidator{TValue}"/> class.
    /// </summary>
    public BlockEditorValueRequiredValidator(IJsonSerializer jsonSerializer) => _jsonSerializer = jsonSerializer;

    /// <inheritdoc/>
    public override IEnumerable<ValidationResult> ValidateRequired(object? value, string? valueType)
    {
        IEnumerable<ValidationResult> validationResults = base.ValidateRequired(value, valueType);

        if (value is null)
        {
            return validationResults;
        }

        if (_jsonSerializer.TryDeserialize(value, out TValue? blockValue) && HoldsNoBlocks(blockValue))
        {
            validationResults = validationResults.Append(new ValidationResult(Constants.Validation.ErrorMessages.Properties.Empty, ["value"]));
        }

        return validationResults;
    }

    private static bool HoldsNoBlocks(BlockValue blockValue)

        // An emptied block editor retains its layout entry, so the blocks within each layout have to be counted
        // rather than the layout itself.
        => blockValue.ContentData.Count == 0
           && blockValue.Layout.Values.All(layoutItems => layoutItems.Any() is false);
}
