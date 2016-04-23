using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
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
            var path = valueSet.Values[BaseUmbracoIndexer.IndexPathFieldName];
            // Test for access if we're only indexing published content
            // return nothing if we're not supporting protected content and it is protected, and we're not supporting unpublished content
            if (_options.SupportUnpublishedContent == false
                && (_options.SupportProtectedContent == false
                    && path != null
                    && _publicAccessService.IsProtected(path.First().ToString())))
            {
                return false;
            }
            return true;
        }
    }
}