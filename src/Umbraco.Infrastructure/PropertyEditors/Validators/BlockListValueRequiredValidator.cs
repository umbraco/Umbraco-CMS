using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.Validators;

/// <summary>
/// Custom validator for block value required validation.
/// </summary>
internal class BlockListValueRequiredValidator : RequiredValidator, IValueRequiredValidator
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockListValueRequiredValidator"/> class.
    /// </summary>
    public BlockListValueRequiredValidator(IJsonSerializer jsonSerializer)
        : base() => _jsonSerializer = jsonSerializer;

    /// <inheritdoc/>
    public override IEnumerable<ValidationResult> ValidateRequired(object? value, string? valueType)
    {
        IEnumerable<ValidationResult> validationResults = base.ValidateRequired(value, valueType);

        if (value is null)
        {
            return validationResults;
        }

        if (_jsonSerializer.TryDeserialize(value, out BlockListValue? blockListValue) &&
            blockListValue.ContentData.Count == 0 &&
            blockListValue.Layout.Count == 0)
        {
            validationResults = validationResults.Append(new ValidationResult(Constants.Validation.ErrorMessages.Properties.Empty, ["value"]));
        }

        return validationResults;
    }
}
