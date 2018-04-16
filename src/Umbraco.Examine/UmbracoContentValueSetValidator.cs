using System;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Umbraco.Examine
{
    /// <summary>
    /// Used to validate a ValueSet for content - based on permissions, parent id, etc....
    /// </summary>
    public class UmbracoContentValueSetValidator : IValueSetValidator
    {
        private readonly UmbracoContentIndexerOptions _options;
        private readonly IPublicAccessService _publicAccessService;
        private const string PathKey = "path";
        public UmbracoContentValueSetValidator(UmbracoContentIndexerOptions options, IPublicAccessService publicAccessService)
        {
            _options = options;
            _publicAccessService = publicAccessService;
        }

        public bool Validate(ValueSet valueSet)
        {
            //check for published content
            if (valueSet.Category == IndexTypes.Content
                && valueSet.Values.ContainsKey(UmbracoExamineIndexer.PublishedFieldName))
            {
                var published = valueSet.Values[UmbracoExamineIndexer.PublishedFieldName] != null && valueSet.Values[UmbracoExamineIndexer.PublishedFieldName][0].Equals(1);
                //we don't support unpublished and the item is not published return false
                if (_options.SupportUnpublishedContent == false && published == false)
                {
                    return false;
                }
            }

            //must have a 'path'
            if (valueSet.Values.ContainsKey(PathKey) == false) return false;
            var path = valueSet.Values[PathKey]?[0].ToString() ?? string.Empty;

            // Test for access if we're only indexing published content
            // return nothing if we're not supporting protected content and it is protected, and we're not supporting unpublished content
            if (valueSet.Category == IndexTypes.Content
                && _options.SupportUnpublishedContent == false
                && _options.SupportProtectedContent == false
                && _publicAccessService.IsProtected(path))
            {
                return false;
            }

            //check if this document is a descendent of the parent
            if (_options.ParentId.HasValue && _options.ParentId.Value > 0)
            {
                if (path.IsNullOrWhiteSpace()) return false;
                if (path.Contains(string.Concat(",", _options.ParentId.Value, ",")) == false)
                    return false;
            }

            //check for recycle bin
            if (_options.SupportUnpublishedContent == false)
            {
                if (path.IsNullOrWhiteSpace()) return false;
                var recycleBinId = valueSet.Category == IndexTypes.Content ? Constants.System.RecycleBinContent : Constants.System.RecycleBinMedia;
                if (path.Contains(string.Concat(",", recycleBinId, ",")))
                    return false;
            }

            return true;
        }
    }
}
