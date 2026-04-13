using System.Globalization;
using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Used to validate a ValueSet for content/media - based on permissions, parent id, etc....
/// </summary>
public class ContentValueSetValidator : ValueSetValidator, IContentValueSetValidator
{
    private const string PathKey = "path";
    private static readonly IEnumerable<string> ValidCategories = new[] { IndexTypes.Content, IndexTypes.Media };
    private readonly IPublicAccessService? _publicAccessService;
    private readonly IScopeProvider? _scopeProvider;

    // used for tests
    internal ContentValueSetValidator(bool publishedValuesOnly, int? parentId = null, IEnumerable<string>? includeItemTypes = null, IEnumerable<string>? excludeItemTypes = null)
        : this(publishedValuesOnly, true, null, null, parentId, includeItemTypes, excludeItemTypes)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentValueSetValidator"/> class.
    /// </summary>
    /// <param name="publishedValuesOnly">If <c>true</c>, only published values are validated.</param>
    /// <param name="supportProtectedContent">If <c>true</c>, enables validation of protected content.</param>
    /// <param name="publicAccessService">The service used to check public access permissions. May be <c>null</c>.</param>
    /// <param name="scopeProvider">The provider for managing database scopes. May be <c>null</c>.</param>
    /// <param name="parentId">Optional parent content ID to filter by. Defaults to <c>null</c>.</param>
    /// <param name="includeItemTypes">Optional collection of item types to include in validation. Defaults to <c>null</c> (all types included).</param>
    /// <param name="excludeItemTypes">Optional collection of item types to exclude from validation. Defaults to <c>null</c>.</param>
    /// <param name="includeFields">Optional collection of field names to include in validation. Defaults to <c>null</c> (all fields included).</param>
    /// <param name="excludeFields">Optional collection of field names to exclude from validation. Defaults to <c>null</c>.</param>
    public ContentValueSetValidator(
        bool publishedValuesOnly,
        bool supportProtectedContent,
        IPublicAccessService? publicAccessService,
        IScopeProvider? scopeProvider,
        int? parentId = null,
        IEnumerable<string>? includeItemTypes = null,
        IEnumerable<string>? excludeItemTypes = null,
        IEnumerable<string>? includeFields = null,
        IEnumerable<string>? excludeFields = null)
        : base(includeItemTypes, excludeItemTypes, includeFields, excludeFields)
    {
        PublishedValuesOnly = publishedValuesOnly;
        SupportProtectedContent = supportProtectedContent;
        ParentId = parentId;
        _publicAccessService = publicAccessService;
        _scopeProvider = scopeProvider;
    }

    protected override IEnumerable<string> ValidIndexCategories => ValidCategories;

    /// <summary>
    /// Gets a value indicating whether the validator includes only published content values.
    /// </summary>
    public bool PublishedValuesOnly { get; }

    /// <summary>
    /// Gets a value indicating whether this validator supports protected content.
    /// </summary>
    public bool SupportProtectedContent { get; }

    /// <summary>
    /// Gets the identifier of the parent content item, if available.
    /// </summary>
    public int? ParentId { get; }

    /// <summary>
    /// Determines whether the specified <paramref name="path"/> represents a descendant of the configured parent, based on the provided <paramref name="category"/>.
    /// </summary>
    /// <param name="path">The comma-delimited string representing the content path to validate.</param>
    /// <param name="category">The category of the content, used for validation context.</param>
    /// <returns>
    /// <c>true</c> if the <paramref name="path"/> contains the parent ID and is considered valid; otherwise, <c>false</c>.
    /// </returns>
    public bool ValidatePath(string path, string category)
    {
        //check if this document is a descendent of the parent
        if (ParentId.HasValue && ParentId.Value > 0)
        {
            // we cannot return FAILED here because we need the value set to get into the indexer and then deal with it from there
            // because we need to remove anything that doesn't pass by parent Id in the cases that umbraco data is moved to an illegal parent.
            if (!path.Contains(string.Concat(",", ParentId.Value.ToString(CultureInfo.InvariantCulture), ",")))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified path is outside of the recycle bin for the given category.
    /// </summary>
    /// <param name="path">The comma-delimited path string to validate.</param>
    /// <param name="category">The category (e.g., content or media) used to determine the appropriate recycle bin.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="path"/> does not include the recycle bin for the specified <paramref name="category"/> (when <c>PublishedValuesOnly</c> is <c>true</c>); otherwise, <c>false</c>.
    /// </returns>
    public bool ValidateRecycleBin(string path, string category)
    {
        var recycleBinId = category == IndexTypes.Content
            ? Constants.System.RecycleBinContentString
            : Constants.System.RecycleBinMediaString;

        //check for recycle bin
        if (PublishedValuesOnly)
        {
            if (path.Contains(string.Concat(",", recycleBinId, ",")))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether the content at the specified path is allowed to be indexed based on its protection status and the given category.
    /// </summary>
    /// <param name="path">The content path to check for protection.</param>
    /// <param name="category">The category of the content, typically the index type (e.g., <c>IndexTypes.Content</c>).</param>
    /// <returns>
    /// <c>true</c> if the content is not protected, protection is not supported, or the category does not require protection checks; otherwise, <c>false</c>.
    /// </returns>
    public bool ValidateProtectedContent(string path, string category)
    {
        if (category == IndexTypes.Content && !SupportProtectedContent)
        {
            //if the service is null we can't look this up so we'll return false
            if (_publicAccessService == null || _scopeProvider == null)
            {
                return false;
            }

            // explicit scope since we may be in a background thread
            using (_scopeProvider.CreateScope(autoComplete: true))
            {
                if (_publicAccessService.IsProtected(path).Success)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Validates the specified <see cref="Umbraco.Cms.Core.Models.ValueSet"/> to determine if it meets the requirements for indexing in Examine.
    /// <para>
    /// Validation includes checks for published status (including culture variants), the presence and validity of the content path, and whether the content is in the recycle bin or protected.
    /// If the value set fails any of these checks, it is either marked as failed (not indexable) or filtered (excluded from the index), depending on the nature of the issue.
    /// </para>
    /// </summary>
    /// <param name="valueSet">The <see cref="Umbraco.Cms.Core.Models.ValueSet"/> to validate.</param>
    /// <returns>
    /// A <see cref="Umbraco.Cms.Infrastructure.Examine.ValueSetValidationResult"/> indicating the outcome of the validation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>Valid</c>: The value set passed all checks and is suitable for indexing.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>Filtered</c>: The value set should be excluded from the index due to path, recycle bin, or protected content rules.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>Failed</c>: The value set is invalid (for example, missing required fields) and cannot be indexed.</description>
    ///   </item>
    /// </list>
    /// The result may also contain a filtered version of the original value set with unpublished culture variants removed.
    /// </returns>
    public override ValueSetValidationResult Validate(ValueSet valueSet)
    {
        // Notes on status on the result:
        // A result status of filtered means that this whole value set result is to be filtered from the index
        // For example the path is incorrect or it is in the recycle bin
        // It does not mean that the values it contains have been through a filtering (for example if an language variant is not published)
        // See notes on issue 11383

        ValueSetValidationResult baseValidate = base.Validate(valueSet);
        valueSet = baseValidate.ValueSet;
        if (baseValidate.Status == ValueSetValidationStatus.Failed)
        {
            return new ValueSetValidationResult(ValueSetValidationStatus.Failed, valueSet);
        }

        var filteredValues = valueSet.Values.ToDictionary(x => x.Key, x => x.Value.ToList());
        //check for published content
        if (valueSet.Category == IndexTypes.Content && PublishedValuesOnly)
        {
            if (!valueSet.Values.TryGetValue(UmbracoExamineFieldNames.PublishedFieldName, out IReadOnlyList<object>? published))
            {
                return new ValueSetValidationResult(ValueSetValidationStatus.Failed, valueSet);
            }

            if (!published[0].Equals("y"))
            {
                return new ValueSetValidationResult(ValueSetValidationStatus.Failed, valueSet);
            }

            //deal with variants, if there are unpublished variants than we need to remove them from the value set
            if (valueSet.Values.TryGetValue(UmbracoExamineFieldNames.VariesByCultureFieldName, out IReadOnlyList<object>? variesByCulture)
                && variesByCulture.Count > 0 && variesByCulture[0].Equals("y"))
            {
                //so this valueset is for a content that varies by culture, now check for non-published cultures and remove those values
                foreach (KeyValuePair<string, IReadOnlyList<object>> publishField in valueSet.Values.Where(x => x.Key.StartsWith($"{UmbracoExamineFieldNames.PublishedFieldName}_")).ToList())
                {
                    if (publishField.Value.Count <= 0 || !publishField.Value[0].Equals("y"))
                    {
                        //this culture is not published, so remove all of these culture values
                        var cultureSuffix = publishField.Key.Substring(publishField.Key.LastIndexOf('_'));
                        foreach (KeyValuePair<string, IReadOnlyList<object>> cultureField in valueSet.Values.Where(x => x.Key.InvariantEndsWith(cultureSuffix)).ToList())
                        {
                            filteredValues.Remove(cultureField.Key);
                        }
                    }
                }
            }
        }

        //must have a 'path'
        if (!valueSet.Values.TryGetValue(PathKey, out IReadOnlyList<object>? pathValues))
        {
            return new ValueSetValidationResult(ValueSetValidationStatus.Failed, valueSet);
        }

        if (pathValues.Count == 0)
        {
            return new ValueSetValidationResult(ValueSetValidationStatus.Failed, valueSet);
        }

        if (pathValues[0] == null)
        {
            return new ValueSetValidationResult(ValueSetValidationStatus.Failed, valueSet);
        }

        if (pathValues[0].ToString().IsNullOrWhiteSpace())
        {
            return new ValueSetValidationResult(ValueSetValidationStatus.Failed, valueSet);
        }

        var path = pathValues[0].ToString();

        var filteredValueSet = new ValueSet(valueSet.Id, valueSet.Category, valueSet.ItemType, filteredValues.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value));

        // We need to validate the path of the content based on ParentId, protected content and recycle bin rules.
        // We cannot return FAILED here because we need the value set to get into the indexer and then deal with it from there
        // because we need to remove anything that doesn't pass by protected content in the cases that umbraco data is moved to an illegal parent.
        // Therefore we return FILTERED to indicate this whole set needs to be filtered out
        if (!ValidatePath(path!, valueSet.Category)
            || !ValidateRecycleBin(path!, valueSet.Category)
            || !ValidateProtectedContent(path!, valueSet.Category))
        {
            return new ValueSetValidationResult(ValueSetValidationStatus.Filtered, filteredValueSet);
        }

        return new ValueSetValidationResult(ValueSetValidationStatus.Valid, filteredValueSet);
    }
}
