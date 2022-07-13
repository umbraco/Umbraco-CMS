using Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Performing basic validation of a value set
/// </summary>
public class ValueSetValidator : IValueSetValidator
{
    public ValueSetValidator(
        IEnumerable<string>? includeItemTypes,
        IEnumerable<string>? excludeItemTypes,
        IEnumerable<string>? includeFields,
        IEnumerable<string>? excludeFields)
    {
        IncludeItemTypes = includeItemTypes;
        ExcludeItemTypes = excludeItemTypes;
        IncludeFields = includeFields;
        ExcludeFields = excludeFields;
        ValidIndexCategories = null;
    }

    /// <summary>
    ///     Optional inclusion list of content types to index
    /// </summary>
    /// <remarks>
    ///     All other types will be ignored if they do not match this list
    /// </remarks>
    public IEnumerable<string>? IncludeItemTypes { get; }

    /// <summary>
    ///     Optional exclusion list of content types to ignore
    /// </summary>
    /// <remarks>
    ///     Any content type alias matched in this will not be included in the index
    /// </remarks>
    public IEnumerable<string>? ExcludeItemTypes { get; }

    /// <summary>
    ///     Optional inclusion list of index fields to index
    /// </summary>
    /// <remarks>
    ///     If specified, all other fields in a <see cref="ValueSet" /> will be filtered
    /// </remarks>
    public IEnumerable<string>? IncludeFields { get; }

    /// <summary>
    ///     Optional exclusion list of index fields
    /// </summary>
    /// <remarks>
    ///     If specified, all fields matching these field names will be filtered from the <see cref="ValueSet" />
    /// </remarks>
    public IEnumerable<string>? ExcludeFields { get; }

    protected virtual IEnumerable<string>? ValidIndexCategories { get; }

    public virtual ValueSetValidationResult Validate(ValueSet valueSet)
    {
        if (ValidIndexCategories != null && !ValidIndexCategories.InvariantContains(valueSet.Category))
        {
            return new ValueSetValidationResult(ValueSetValidationStatus.Failed, valueSet);
        }

        // check if this document is of a correct type of node type alias
        if (IncludeItemTypes != null && !IncludeItemTypes.InvariantContains(valueSet.ItemType))
        {
            return new ValueSetValidationResult(ValueSetValidationStatus.Failed, valueSet);
        }

        // if this node type is part of our exclusion list
        if (ExcludeItemTypes != null && ExcludeItemTypes.InvariantContains(valueSet.ItemType))
        {
            return new ValueSetValidationResult(ValueSetValidationStatus.Failed, valueSet);
        }

        var isFiltered = false;

        var filteredValues = valueSet.Values.ToDictionary(x => x.Key, x => x.Value.ToList());

        // filter based on the fields provided (if any)
        if (IncludeFields != null || ExcludeFields != null)
        {
            foreach (var key in valueSet.Values.Keys.ToList())
            {
                if (IncludeFields != null && !IncludeFields.InvariantContains(key))
                {
                    filteredValues.Remove(key); // remove any value with a key that doesn't match the inclusion list
                    isFiltered = true;
                }

                if (ExcludeFields != null && ExcludeFields.InvariantContains(key))
                {
                    filteredValues.Remove(key); // remove any value with a key that matches the exclusion list
                    isFiltered = true;
                }
            }
        }

        var filteredValueSet = new ValueSet(valueSet.Id, valueSet.Category, valueSet.ItemType, filteredValues.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value));
        return new ValueSetValidationResult(
            isFiltered ? ValueSetValidationStatus.Filtered : ValueSetValidationStatus.Valid, filteredValueSet);
    }
}
