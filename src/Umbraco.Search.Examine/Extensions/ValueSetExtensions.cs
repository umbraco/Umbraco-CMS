using Examine;
using Umbraco.Search.ValueSet;
using Umbraco.Search.ValueSet.Validators;

namespace Umbraco.Search.Examine.Extensions;

public static class ValueSetExtensions
{
    public static UmbracoValueSet ToUmbracoValueSet(this global::Examine.ValueSet valueSet)
    {
        return new UmbracoValueSet(valueSet.Id, valueSet.Category,
            valueSet.Values.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value));
    }
    public static global::Examine.ValueSet ToExamineValueSet(this UmbracoValueSet valueSet)
    {
        return new global::Examine.ValueSet(valueSet.Id, valueSet.Category,
            valueSet.Values?.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value));
    }
    public static global::Examine.ValueSetValidationResult ToExamineValidationResult(this UmbracoValueSetValidationResult valueSet)
    {
        return new global::Examine.ValueSetValidationResult(valueSet.Status.ToExamineStatus(), valueSet.ValueSet.ToExamineValueSet());
    }
    public static global::Examine.ValueSetValidationStatus ToExamineStatus(this UmbracoValueSetValidationStatus status)
    {
        return status switch
        {
            UmbracoValueSetValidationStatus.Valid => ValueSetValidationStatus.Valid,
            UmbracoValueSetValidationStatus.Failed => ValueSetValidationStatus.Failed,
            UmbracoValueSetValidationStatus.Filtered => ValueSetValidationStatus.Filtered
        };
    }
}
