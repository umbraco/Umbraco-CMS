using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Umbraco.Examine
{
    /// <summary>
    /// Used to validate a ValueSet for content/media - based on permissions, parent id, etc....
    /// </summary>
    public class ContentValueSetValidator : ValueSetValidator, IContentValueSetValidator
    {
        private readonly IPublicAccessService _publicAccessService;
        private readonly IScopeProvider _scopeProvider;
        private const string PathKey = "path";
        private static readonly IEnumerable<string> ValidCategories = new[] { IndexTypes.Content, IndexTypes.Media };
        protected override IEnumerable<string> ValidIndexCategories => ValidCategories;

        public bool PublishedValuesOnly { get; }
        public bool SupportProtectedContent { get; }
        public int? ParentId { get; }

        public bool ValidatePath(string path, string category)
        {
            //check if this document is a descendent of the parent
            if (ParentId.HasValue && ParentId.Value > 0)
            {
                // we cannot return FAILED here because we need the value set to get into the indexer and then deal with it from there
                // because we need to remove anything that doesn't pass by parent Id in the cases that umbraco data is moved to an illegal parent.
                if (!path.Contains(string.Concat(",", ParentId.Value, ",")))
                    return false;
            }

            return true;
        }

        public bool ValidateRecycleBin(string path, string category)
        {
            var recycleBinId = category == IndexTypes.Content ? Constants.System.RecycleBinContent : Constants.System.RecycleBinMedia;

            //check for recycle bin
            if (PublishedValuesOnly)
            {
                if (path.Contains(string.Concat(",", recycleBinId, ",")))
                    return false;
            }
            return true;
        }

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
                    if (_publicAccessService.IsProtected(path))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // used for tests
        public ContentValueSetValidator(bool publishedValuesOnly, int? parentId = null,
            IEnumerable<string> includeItemTypes = null, IEnumerable<string> excludeItemTypes = null)
            : this(publishedValuesOnly, true, null, null, parentId, includeItemTypes, excludeItemTypes)
        {
        }

        public ContentValueSetValidator(bool publishedValuesOnly, bool supportProtectedContent,
            IPublicAccessService publicAccessService,
            IScopeProvider scopeProvider,
            int? parentId = null,
            IEnumerable<string> includeItemTypes = null, IEnumerable<string> excludeItemTypes = null)
            : base(includeItemTypes, excludeItemTypes, null, null)
        {
            PublishedValuesOnly = publishedValuesOnly;
            SupportProtectedContent = supportProtectedContent;
            ParentId = parentId;
            _publicAccessService = publicAccessService;
            _scopeProvider = scopeProvider;
        }

        [Obsolete("Use the ctor with all parameters instead")]
        public ContentValueSetValidator(bool publishedValuesOnly, bool supportProtectedContent,
            IPublicAccessService publicAccessService, int? parentId = null,
            IEnumerable<string> includeItemTypes = null, IEnumerable<string> excludeItemTypes = null)
            : this(publishedValuesOnly, supportProtectedContent, publicAccessService, Current.ScopeProvider,
                  parentId, includeItemTypes, excludeItemTypes)
        {
        }

        public override ValueSetValidationResult Validate(ValueSet valueSet)
        {
            var baseValidate = base.Validate(valueSet);
            if (baseValidate == ValueSetValidationResult.Failed)
                return ValueSetValidationResult.Failed;

            var isFiltered = baseValidate == ValueSetValidationResult.Filtered;

            //check for published content
            if (valueSet.Category == IndexTypes.Content && PublishedValuesOnly)
            {
                if (!valueSet.Values.TryGetValue(UmbracoExamineIndex.PublishedFieldName, out var published))
                    return ValueSetValidationResult.Failed;

                if (!published[0].Equals("y"))
                    return ValueSetValidationResult.Failed;

                //deal with variants, if there are unpublished variants than we need to remove them from the value set
                if (valueSet.Values.TryGetValue(UmbracoContentIndex.VariesByCultureFieldName, out var variesByCulture)
                    && variesByCulture.Count > 0 && variesByCulture[0].Equals("y"))
                {
                    //so this valueset is for a content that varies by culture, now check for non-published cultures and remove those values
                    foreach (var publishField in valueSet.Values.Where(x => x.Key.StartsWith($"{UmbracoExamineIndex.PublishedFieldName}_")).ToList())
                    {
                        if (publishField.Value.Count <= 0 || !publishField.Value[0].Equals("y"))
                        {
                            //this culture is not published, so remove all of these culture values
                            var cultureSuffix = publishField.Key.Substring(publishField.Key.LastIndexOf('_'));
                            foreach (var cultureField in valueSet.Values.Where(x => x.Key.InvariantEndsWith(cultureSuffix)).ToList())
                            {
                                valueSet.Values.Remove(cultureField.Key);
                                isFiltered = true;
                            }
                        }
                    }
                }
            }

            //must have a 'path'
            if (!valueSet.Values.TryGetValue(PathKey, out var pathValues)) return ValueSetValidationResult.Failed;
            if (pathValues.Count == 0) return ValueSetValidationResult.Failed;
            if (pathValues[0] == null) return ValueSetValidationResult.Failed;
            if (pathValues[0].ToString().IsNullOrWhiteSpace()) return ValueSetValidationResult.Failed;
            var path = pathValues[0].ToString();

            // We need to validate the path of the content based on ParentId, protected content and recycle bin rules.
            // We cannot return FAILED here because we need the value set to get into the indexer and then deal with it from there
            // because we need to remove anything that doesn't pass by protected content in the cases that umbraco data is moved to an illegal parent.
            if (!ValidatePath(path, valueSet.Category)
                || !ValidateRecycleBin(path, valueSet.Category)
                || !ValidateProtectedContent(path, valueSet.Category))
                return ValueSetValidationResult.Filtered;

            return isFiltered ? ValueSetValidationResult.Filtered : ValueSetValidationResult.Valid;
        }
    }
}
