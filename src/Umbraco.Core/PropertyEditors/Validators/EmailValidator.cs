using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
///     A validator that validates an email address
/// </summary>
public sealed class EmailValidator : IManifestValueValidator
{
    /// <inheritdoc />
    public string ValidationName => "Email";

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration)
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
