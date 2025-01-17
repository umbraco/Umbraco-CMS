using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
///     A validator that validates an email address
/// </summary>
public sealed class EmailValidator : IValueValidator
{
    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
    {
        var asString = value == null ? string.Empty : value.ToString();

        var emailVal = new EmailAddressAttribute();

        if (asString != string.Empty && emailVal.IsValid(asString) == false)
        {
            // TODO: localize these!
            yield return new ValidationResult("Email is invalid", new[] { "value" });
        }
    }
}
