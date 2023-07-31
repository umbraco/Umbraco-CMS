using Examine;
using Umbraco.Search.Examine.Extensions;
using Umbraco.Search.ValueSet.Validators;

namespace Umbraco.Search.Examine.ValueSet;

public class ExamineValueSetValidator : IValueSetValidator
{
    private readonly IUmbracoValueSetValidator? _valueSetValidator;

    public ExamineValueSetValidator(IUmbracoValueSetValidator? valueSetValidator)
    {
        _valueSetValidator = valueSetValidator;
    }

    public ValueSetValidationResult Validate(global::Examine.ValueSet valueSet)
    {
        if (_valueSetValidator == null)
        {
            return new ValueSetValidationResult(ValueSetValidationStatus.Valid, valueSet);
        }
        return _valueSetValidator.Validate(valueSet.ToUmbracoValueSet()).ToExamineValidationResult();
    }
}
