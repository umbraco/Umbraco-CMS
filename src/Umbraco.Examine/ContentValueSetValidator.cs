using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Examine
{

    /// <summary>
    /// Used to validate a ValueSet for content/media - based on permissions, parent id, etc....
    /// </summary>
    public class ContentValueSetValidator : IValueSetValidator
    {
        private readonly UmbracoContentIndexerOptions _options;
        private readonly IPublicAccessService _publicAccessService;

        private const string PathKey = "path";
        private static readonly IEnumerable<string> ValidIndexTypes = new[] {IndexTypes.Content, IndexTypes.Media};

        public ContentValueSetValidator(UmbracoContentIndexerOptions options, IPublicAccessService publicAccessService)
        {
            _options = options;
            _publicAccessService = publicAccessService;
        }

        public bool Validate(ValueSet valueSet)
        {
            if (!ValidIndexTypes.Contains(valueSet.Category))
                return false;

            //check for published content
            if (valueSet.Category == IndexTypes.Content && !_options.SupportUnpublishedContent)
            {
                if (!valueSet.Values.TryGetValue(UmbracoExamineIndexer.PublishedFieldName, out var published))
                    return false;

                if (!published[0].Equals(1))
                    return false;

                //deal with variants, if there are unpublished variants than we need to remove them from the value set
                if (valueSet.Values.TryGetValue(UmbracoContentIndexer.VariesByCultureFieldName, out var variesByCulture)
                    && variesByCulture.Count > 0 && variesByCulture[0].Equals(1))
                {
                    //so this valueset is for a content that varies by culture, now check for non-published cultures and remove those values
                    foreach(var publishField in valueSet.Values.Where(x => x.Key.StartsWith($"{UmbracoExamineIndexer.PublishedFieldName}_")).ToList())
                    {
                        if (publishField.Value.Count <= 0 || !publishField.Value[0].Equals(1))
                        {
                            //this culture is not published, so remove all of these culture values
                            var cultureSuffix = publishField.Key.Substring(publishField.Key.LastIndexOf('_'));
                            foreach (var cultureField in valueSet.Values.Where(x => x.Key.InvariantEndsWith(cultureSuffix)).ToList())
                            {
                                valueSet.Values.Remove(cultureField.Key);
                            }
                        }
                    }
                }
            }

            //must have a 'path'
            if (!valueSet.Values.TryGetValue(PathKey, out var pathValues)) return false;
            if (pathValues.Count == 0) return false;
            if (pathValues[0] == null) return false;
            if (pathValues[0].ToString().IsNullOrWhiteSpace()) return false;
            var path = pathValues[0].ToString();

            // return nothing if we're not supporting protected content and it is protected, and we're not supporting unpublished content
            if (valueSet.Category == IndexTypes.Content
                && !_options.SupportProtectedContent
                && _publicAccessService.IsProtected(path))
            {
                return false;
            }

            //check if this document is a descendent of the parent
            if (_options.ParentId.HasValue && _options.ParentId.Value > 0)
            {
                if (!path.Contains(string.Concat(",", _options.ParentId.Value, ",")))
                    return false;
            }

            //check for recycle bin
            if (!_options.SupportUnpublishedContent)
            {
                var recycleBinId = valueSet.Category == IndexTypes.Content ? Constants.System.RecycleBinContent : Constants.System.RecycleBinMedia;
                if (path.Contains(string.Concat(",", recycleBinId, ",")))
                    return false;
            }

            //check if this document is of a correct type of node type alias
            if (_options.IncludeContentTypes != null && !_options.IncludeContentTypes.Contains(valueSet.ItemType))
                return false;

            //if this node type is part of our exclusion list
            if (_options.ExcludeContentTypes != null && _options.ExcludeContentTypes.Contains(valueSet.ItemType))
                return false;

            return true;
        }
    }
}
