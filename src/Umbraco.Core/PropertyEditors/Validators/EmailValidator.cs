using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
/// A validator that validates an email address.
/// </summary>
public sealed class EmailValidator : IValueValidator
{
    private readonly ILocalizedTextService _localizedTextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailValidator"/> class.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 17.")]
    public EmailValidator()
        : this(StaticServiceProvider.Instance.GetRequiredService<ILocalizedTextService>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailValidator"/> class.
    /// </summary>
    public EmailValidator(ILocalizedTextService localizedTextService) => _localizedTextService = localizedTextService;

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
    {
        var valueAsString = value?.ToString() ?? string.Empty;

        var emailAddressAttribute = new EmailAddressAttribute();

        if (valueAsString != string.Empty && emailAddressAttribute.IsValid(valueAsString) == false)
        {
            yield return new ValidationResult(
                _localizedTextService.Localize("validation", "invalidEmail", [valueAsString]),
                ["value"]);
        }
    }
}
