namespace Umbraco.Search.ValueSet.Validators;

public interface IUmbracoValueSetValidator
{
    UmbracoValueSetValidationResult Validate(UmbracoValueSet valueSet);
}

public class UmbracoValueSetValidationResult
{
    public UmbracoValueSetValidationResult(UmbracoValueSetValidationStatus status, UmbracoValueSet valueSet)
    {
        Status = status;
        ValueSet = valueSet;
    }

    public UmbracoValueSetValidationStatus Status { get; }

    public UmbracoValueSet ValueSet { get; }
}

/// <summary>
///
/// </summary>
public enum UmbracoValueSetValidationStatus
{
    /// <summary>
    /// If the result is valid
    /// </summary>
    Valid,

    /// <summary>
    /// If validation failed, the value set will not be included in the index
    /// </summary>
    Failed,

    /// <summary>
    /// If validation passed but the value set was filtered
    /// </summary>
    Filtered
}
