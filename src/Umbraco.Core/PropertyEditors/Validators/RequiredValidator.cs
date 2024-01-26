using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
///     A validator that validates that the value is not null or empty (if it is a string)
/// </summary>
public sealed class RequiredValidator : IValueRequiredValidator, IManifestValueValidator
{
    [Obsolete($"Use the constructor that does not accept {nameof(ILocalizedTextService)}. Will be removed in V15.")]
    public RequiredValidator(ILocalizedTextService textService)
        : this()
    {
    }

    public RequiredValidator()
    {
    }

    /// <inheritdoc cref="IManifestValueValidator.ValidationName" />
    public string ValidationName => "Required";

    /// <inheritdoc cref="IValueValidator.Validate" />
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration) =>
        ValidateRequired(value, valueType);

    /// <inheritdoc cref="IValueRequiredValidator.ValidateRequired" />
    public IEnumerable<ValidationResult> ValidateRequired(object? value, string? valueType)
    {
        if (value == null)
        {
            yield return new ValidationResult(Constants.Validation.ErrorMessages.Properties.Missing, new[] { "value" });
            yield break;
        }

        if (valueType.InvariantEquals(ValueTypes.Json))
        {
            if (value.ToString()?.DetectIsEmptyJson() ?? false)
            {
                yield return new ValidationResult(Constants.Validation.ErrorMessages.Properties.Empty, new[] { "value" });
            }

            yield break;
        }

        if (value.ToString().IsNullOrWhiteSpace())
        {
            yield return new ValidationResult(Constants.Validation.ErrorMessages.Properties.Empty, new[] { "value" });
        }
    }
}
