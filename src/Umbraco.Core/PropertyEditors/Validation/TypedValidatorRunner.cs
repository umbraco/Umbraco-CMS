using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
/// <para>
/// An aggregate validator for value editors that provide already-typed values (not JSON).
/// </para>
/// <para>
/// Will cast the configuration once, and pass the typed value to each <see cref="ITypedJsonValidator{TValue,TConfiguration}"/>, aggregating the results.
/// </para>
/// </summary>
/// <typeparam name="TValue">The type of the expected value.</typeparam>
/// <typeparam name="TConfiguration">The type of the expected configuration.</typeparam>
public class TypedValidatorRunner<TValue, TConfiguration> : IValueValidator
    where TValue : class
{
    private readonly ITypedJsonValidator<TValue, TConfiguration>[] _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypedValidatorRunner{TValue, TConfiguration}"/> class.
    /// </summary>
    /// <param name="validators">The collection of validators to run.</param>
    public TypedValidatorRunner(params ITypedJsonValidator<TValue, TConfiguration>[] validators)
        => _validators = validators;

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(
        object? value,
        string? valueType,
        object? dataTypeConfiguration,
        PropertyValidationContext validationContext)
    {
        if (dataTypeConfiguration is not TConfiguration configuration)
        {
            return [];
        }

        if (value is not null and not TValue)
        {
            return [];
        }

        var typedValue = value as TValue;

        return _validators
            .SelectMany(v => v.Validate(typedValue, configuration, valueType, validationContext))
            .ToList();
    }
}
