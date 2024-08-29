using Examine;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine;

public interface IExamineExternalIndexSearcherTest
{

        IEnumerable<ISearchResult> Search(
            string query,
            UmbracoEntityTypes entityType,
            int pageSize,
            long pageIndex,
            out long totalFound,
            string? searchFrom = null,
            bool ignoreUserStartNodes = false);

}
