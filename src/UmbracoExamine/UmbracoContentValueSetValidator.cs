using System;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace UmbracoExamine
{
    /// <summary>
    /// Used to validate a ValueSet for content - based on permissions, parent id, etc.... 
    /// </summary>
    public class UmbracoContentValueSetValidator : IValueSetValidator
    {
        private readonly UmbracoContentIndexerOptions _options;
        private readonly IPublicAccessService _publicAccessService;

        public UmbracoContentValueSetValidator(UmbracoContentIndexerOptions options, IPublicAccessService publicAccessService)
        {
            _options = options;
            _publicAccessService = publicAccessService;
        }

        public bool Validate(ValueSet valueSet)
        {
            //must have a 'path'
            if (valueSet.Values.ContainsKey("path") == false) return false;

            var path = valueSet.Values["path"] == null ? string.Empty : valueSet.Values["path"][0].ToString();

            
            // Test for access if we're only indexing published content
            // return nothing if we're not supporting protected content and it is protected, and we're not supporting unpublished content
            if (_options.SupportUnpublishedContent == false
                && (_options.SupportProtectedContent == false
                    && path != null
                    && _publicAccessService.IsProtected(path)))
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

            return true;
        }
    }
}