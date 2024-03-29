using Examine;
using Examine.Lucene.Providers;
using Examine.Lucene.Search;
using Examine.Search;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Api.Delivery.Services.QueryBuilders
{
    internal sealed class ApiContentQueryFactory : IApiContentQueryFactory
    {
        /// <inheritdoc/>
        public IQuery CreateApiContentQuery(IIndex index)
        {
            // Needed for enabling leading wildcards searches
            BaseLuceneSearcher searcher = index.Searcher as BaseLuceneSearcher ?? throw new InvalidOperationException($"Index searcher must be of type {nameof(BaseLuceneSearcher)}.");

            IQuery query = searcher.CreateQuery(
                IndexTypes.Content,
                BooleanOperation.And,
                searcher.LuceneAnalyzer,
                new LuceneSearchOptions { AllowLeadingWildcard = true });

            return query;
        }
    }
}
