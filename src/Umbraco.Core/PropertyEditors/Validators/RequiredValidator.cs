using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
///     A validator that validates that the value is not null or empty (if it is a string)
/// </summary>
public sealed class RequiredValidator : IValueRequiredValidator, IManifestValueValidator
{
    private const string ValueCannotBeNull = "Value cannot be null";
    private const string ValueCannotBeEmpty = "Value cannot be empty";
    private readonly ILocalizedTextService _textService;

    public RequiredValidator(ILocalizedTextService textService) => _textService = textService;

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
            yield return new ValidationResult(
                _textService?.Localize("validation", "invalidNull") ?? ValueCannotBeNull,
                new[] { "value" });
            yield break;
        }

        if (valueType.InvariantEquals(ValueTypes.Json))
        {
            if (value.ToString()?.DetectIsEmptyJson() ?? false)
            {
                yield return new ValidationResult(
                    _textService?.Localize("validation", "invalidEmpty") ?? ValueCannotBeEmpty, new[] { "value" });
            }

            yield break;
        }

        if (value.ToString().IsNullOrWhiteSpace())
        {
            yield return new ValidationResult(
                _textService?.Localize("validation", "invalidEmpty") ?? ValueCannotBeEmpty, new[] { "value" });
        }
    }
}
