using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
/// A value validator editor validating property editors that handle multiple values from a configured list of options.
/// </summary>
public class MultipleValueValidator : IValueValidator
{
    private readonly ILocalizedTextService _localizedTextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleValueValidator"/> class.
    /// </summary>
    [Obsolete("Please use the constructor that takes all parameters. Scheduled for removal in Umbraco 17.")]
    public MultipleValueValidator()
        : this(StaticServiceProvider.Instance.GetRequiredService<ILocalizedTextService>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleValueValidator"/> class.
    /// </summary>
    public MultipleValueValidator(ILocalizedTextService localizedTextService) => _localizedTextService = localizedTextService;

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
    {
        // don't validate if empty
        if (value == null || value.ToString().IsNullOrWhiteSpace())
        {
            yield break;
        }

        if (dataTypeConfiguration is not ValueListConfiguration valueListConfiguration)
        {
            yield break;
        }

        if (value is not IEnumerable<string> values)
        {
            yield break;
        }

        var invalidValues = values
            .Where(x => valueListConfiguration.Items.Contains(x) is false)
            .ToList();

        if (invalidValues.Count == 1)
        {
            yield return new ValidationResult(
                _localizedTextService.Localize("validation", "notOneOfOptions", [invalidValues[0]]),
                ["value"]);
        }
        else if (invalidValues.Count > 1)
        {
            yield return new ValidationResult(
                _localizedTextService.Localize("validation", "multipleNotOneOfOptions", [string.Join(", ", invalidValues)]),
                ["value"]);
        }
    }
}
