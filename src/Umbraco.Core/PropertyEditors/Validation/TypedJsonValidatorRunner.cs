using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
/// <para>
/// An aggregate validator for JSON based value editors, to avoid doing multiple deserialization.
/// </para>
/// <para>
/// Will deserialize once, and cast the configuration once, and pass those values to each <see cref="ITypedJsonValidator{TValue,TConfiguration}"/>, aggregating the results.
/// </para>
/// </summary>
/// <typeparam name="TValue">The type of the expected value.</typeparam>
/// <typeparam name="TConfiguration">The type of the expected configuration</typeparam>
public class TypedJsonValidatorRunner<TValue, TConfiguration> : IValueValidator
    where TValue : class
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ITypedJsonValidator<TValue, TConfiguration>[] _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypedJsonValidatorRunner{TValue, TConfiguration}"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="validators">The collection of validators to run.</param>
    public TypedJsonValidatorRunner(IJsonSerializer jsonSerializer, params ITypedJsonValidator<TValue, TConfiguration>[] validators)
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

        foreach (ITypedJsonValidator<TValue, TConfiguration> validator in _validators)
        {
            validationResults.AddRange(validator.Validate(deserializedValue, configuration, valueType, validationContext));
        }

        return validationResults;
    }
}
