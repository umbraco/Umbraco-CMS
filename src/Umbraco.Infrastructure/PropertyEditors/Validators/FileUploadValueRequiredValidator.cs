using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.Validators;

/// <summary>
/// Custom validator for block value required validation.
/// </summary>
internal sealed class FileUploadValueRequiredValidator : RequiredValidator
{
    /// <inheritdoc/>
    public override IEnumerable<ValidationResult> ValidateRequired(object? value, string? valueType)
    {
        IEnumerable<ValidationResult> validationResults = base.ValidateRequired(value, valueType);

        if (value is null)
        {
            return validationResults;
        }

        if (value is JsonObject jsonObject && jsonObject.TryGetPropertyValue("src", out JsonNode? source))
        {
            string sourceString = source!.GetValue<string>();
            if (string.IsNullOrEmpty(sourceString))
            {
                validationResults = validationResults.Append(new ValidationResult(Constants.Validation.ErrorMessages.Properties.Empty, ["value"]));
            }
        }

        return validationResults;
    }
}
