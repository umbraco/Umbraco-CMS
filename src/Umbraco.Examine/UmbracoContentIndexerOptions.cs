using System;
using System.Collections.Generic;

namespace Umbraco.Examine
{   

    /// <summary>
    /// Options used to configure the umbraco content indexer
    /// </summary>
    public class UmbracoContentIndexerOptions
    {
        public bool SupportUnpublishedContent { get; private set; }
        public bool SupportProtectedContent { get; private set; }
        public int? ParentId { get; private set; }

        /// <summary>
        /// Optional inclusion list of content types to index
        /// </summary>
        /// <remarks>
        /// All other types will be ignored if they do not match this list
        /// </remarks>
        public IEnumerable<string> IncludeContentTypes { get; private set; }

        /// <summary>
        /// Optional exclusion list of content types to ignore
        /// </summary>
        /// <remarks>
        /// Any content type alias matched in this will not be included in the index
        /// </remarks>
        public IEnumerable<string> ExcludeContentTypes { get; private set; }

        /// <summary>
        /// Creates a new <see cref="UmbracoContentIndexerOptions"/>
        /// </summary>
        /// <param name="supportUnpublishedContent">If the index supports unpublished content</param>
        /// <param name="supportProtectedContent">If the index supports protected content</param>
        /// <param name="parentId">Optional value indicating to only index content below this ID</param>
        /// <param name="includeContentTypes">Optional content type alias inclusion list</param>
        /// <param name="excludeContentTypes">Optional content type alias exclusion list</param>
        public UmbracoContentIndexerOptions(bool supportUnpublishedContent, bool supportProtectedContent,
            int? parentId = null, IEnumerable<string> includeContentTypes = null, IEnumerable<string> excludeContentTypes = null)
        {
            SupportUnpublishedContent = supportUnpublishedContent;
            SupportProtectedContent = supportProtectedContent;
            ParentId = parentId;
            IncludeContentTypes = includeContentTypes;
            ExcludeContentTypes = excludeContentTypes;
        }


    }
}
