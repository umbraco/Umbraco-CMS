using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;
using Umbraco.Search.ValueSet;
using Umbraco.Search.ValueSet.Validators;
using IValueSetValidator = Examine.IValueSetValidator;

namespace Umbraco.Search.Examine;

/// <summary>
///     Performing basic validation of a value set
/// </summary>
public class UmbracoValueSetValidator : IUmbracoValueSetValidator
{
    public UmbracoValueSetValidator(
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

    public virtual UmbracoValueSetValidationResult Validate(UmbracoValueSet valueSet)
    {
        /* Notes on status on the result:
         * A result status of filtered means that this whole value set result is to be filtered from the index
         * For example the path is incorrect or it is in the recycle bin
         * It does not mean that the values it contains have been through a filtering (for example if an language variant is not published)
         * See notes on issue 11383 */

        if (ValidIndexCategories != null && !ValidIndexCategories.InvariantContains(valueSet.Category!))
        {
            return new UmbracoValueSetValidationResult(UmbracoValueSetValidationStatus.Failed, valueSet);
        }

        // check if this document is of a correct type of node type alias
        if (IncludeItemTypes != null && !IncludeItemTypes.InvariantContains(valueSet.ItemType!))
        {
            return new UmbracoValueSetValidationResult(UmbracoValueSetValidationStatus.Failed, valueSet);
        }

        // if this node type is part of our exclusion list
        if (ExcludeItemTypes != null && ExcludeItemTypes.InvariantContains(valueSet.ItemType!))
        {
            return new UmbracoValueSetValidationResult(UmbracoValueSetValidationStatus.Failed, valueSet);
        }

        var filteredValues = valueSet.Values?.ToDictionary(x => x.Key, x => x.Value.ToList());

        // filter based on the fields provided (if any)
        if (IncludeFields != null || ExcludeFields != null)
        {
            foreach (var key in valueSet.Values?.Keys?.ToArray()!)
            {
                if (IncludeFields != null && !IncludeFields.InvariantContains(key))
                {
                    filteredValues?.Remove(key); // remove any value with a key that doesn't match the inclusion list
                }

                if (ExcludeFields != null && ExcludeFields.InvariantContains(key))
                {
                    filteredValues?.Remove(key); // remove any value with a key that matches the exclusion list
                }

            }
        }

        var filteredValueSet = new UmbracoValueSet(valueSet.Id, valueSet.Category!, valueSet.ItemType!, filteredValues?.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value)!);
        return new UmbracoValueSetValidationResult(UmbracoValueSetValidationStatus.Valid, filteredValueSet);
    }
}
