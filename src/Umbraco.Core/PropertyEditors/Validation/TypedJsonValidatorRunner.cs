using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
/// <para>
/// An aggregate <see cref="IValueValidator"/> for JSON based value editors. Deserializes the editor value into
/// <typeparamref name="TValue"/> once (avoiding repeated deserialization), casts the configuration once, and passes both
/// to each <see cref="ITypedValidator{TValue,TConfiguration}"/>, aggregating the results.
/// </para>
/// <para>
/// Use this runner when the editor value reaching validation is raw JSON that must be deserialized before validation —
/// typically an array of complex objects, such as a media picker storing crop data, which the backoffice JSON object
/// converter leaves as un-typed JSON nodes rather than a typed CLR value.
/// </para>
/// <para>
/// When the editor value is already the typed CLR value (so only a cast is needed, with no deserialization) use
/// <see cref="TypedValidatorRunner{TValue,TConfiguration}"/> instead. That is the only difference between the two runners:
/// this one deserializes, the other casts.
/// </para>
/// </summary>
/// <typeparam name="TValue">The type of the expected value.</typeparam>
/// <typeparam name="TConfiguration">The type of the expected configuration</typeparam>
/// <seealso cref="TypedValidatorRunner{TValue,TConfiguration}"/>
public class TypedJsonValidatorRunner<TValue, TConfiguration> : IValueValidator
    where TValue : class
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ITypedValidator<TValue, TConfiguration>[] _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypedJsonValidatorRunner{TValue, TConfiguration}"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="validators">The collection of validators to run.</param>
    [Obsolete("Use the constructor accepting ITypedValidator instances. Scheduled for removal in Umbraco 20.")]
    public TypedJsonValidatorRunner(IJsonSerializer jsonSerializer, params ITypedJsonValidator<TValue, TConfiguration>[] validators)
        : this(jsonSerializer, (ITypedValidator<TValue, TConfiguration>[])validators)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypedJsonValidatorRunner{TValue, TConfiguration}"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="validators">The collection of validators to run.</param>
    public TypedJsonValidatorRunner(IJsonSerializer jsonSerializer, params ITypedValidator<TValue, TConfiguration>[] validators)
    {
        _jsonSerializer = jsonSerializer;
        _validators = validators;
    }

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(
        object? value,
        string? valueType,
        object? dataTypeConfiguration,
        PropertyValidationContext validationContext)
    {
        var validationResults = new List<ValidationResult>();

        if (dataTypeConfiguration is not TConfiguration configuration)
        {
            return validationResults;
        }

        TValue? deserializedValue = null;
        if (value is not null && _jsonSerializer.TryDeserialize(value, out deserializedValue) is false)
        {
            return validationResults;
        }

        foreach (ITypedValidator<TValue, TConfiguration> validator in _validators)
        {
            validationResults.AddRange(validator.Validate(deserializedValue, configuration, valueType, validationContext));
        }

        return validationResults;
    }
}
