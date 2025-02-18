using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

public class TypedJsonValidatorRunner<TValue, TConfiguration> : IValueValidator
    where TValue : class
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ITypedJsonValidator<TValue, TConfiguration>[] _validators;

    public TypedJsonValidatorRunner(IJsonSerializer jsonSerializer, params ITypedJsonValidator<TValue, TConfiguration>[] validators)
    {
        _jsonSerializer = jsonSerializer;
        _validators = validators;
    }

    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration,
        PropertyValidationContext validationContext)
    {
        var validationResults = new List<ValidationResult>();

        if (dataTypeConfiguration is not TConfiguration configuration)
        {
            return validationResults;
        }

        if (value is null || _jsonSerializer.TryDeserialize(value, out TValue? deserializedValue) is false)
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
