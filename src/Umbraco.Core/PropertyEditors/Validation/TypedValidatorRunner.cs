using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
/// <para>
/// An aggregate <see cref="IValueValidator"/> that casts the editor value once and passes it, along with the cast
/// configuration, to each <see cref="ITypedValidator{TValue,TConfiguration}"/>, aggregating the results.
/// </para>
/// <para>
/// Use this runner when the editor value reaching validation is already the typed CLR value (<typeparamref name="TValue"/>),
/// so a cast is all that is needed — for example a content picker (value is a <see cref="string"/>) or an element picker
/// (value is a <c>List&lt;string&gt;</c>, since the backoffice JSON object converter resolves an array of scalars into a typed list).
/// </para>
/// <para>
/// When the editor value is instead raw JSON that must be deserialized into <typeparamref name="TValue"/> before validation —
/// typically an array of complex objects, such as a media picker storing crop data — use <see cref="TypedJsonValidatorRunner{TValue,TConfiguration}"/>
/// instead. That is the only difference between the two runners: this one casts, the other deserializes.
/// </para>
/// </summary>
/// <typeparam name="TValue">The type of the expected value.</typeparam>
/// <typeparam name="TConfiguration">The type of the expected configuration.</typeparam>
/// <seealso cref="TypedJsonValidatorRunner{TValue,TConfiguration}"/>
public class TypedValidatorRunner<TValue, TConfiguration> : IValueValidator
    where TValue : class
{
    private readonly ITypedValidator<TValue, TConfiguration>[] _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypedValidatorRunner{TValue, TConfiguration}"/> class.
    /// </summary>
    /// <param name="validators">The collection of validators to run.</param>
    public TypedValidatorRunner(params ITypedValidator<TValue, TConfiguration>[] validators)
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
