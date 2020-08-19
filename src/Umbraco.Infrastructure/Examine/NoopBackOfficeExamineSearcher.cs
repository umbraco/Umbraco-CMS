using System.Collections.Generic;
using Examine;
using Umbraco.Examine;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Infrastructure.Examine
{
    public class NoopBackOfficeExamineSearcher : IBackOfficeExamineSearcher
    {
        public IEnumerable<ISearchResult> Search(string query, UmbracoEntityTypes entityType, int pageSize, long pageIndex, out long totalFound,
            string searchFrom = null, bool ignoreUserStartNodes = false)
        {
            totalFound = 0;
            return new ISearchResult[0];
        }
    }
}
