using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Faceting;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

[UmbracoTest(Database = UmbracoTestOptions.Database.None)]
internal abstract class ResolverTestsBase<TResolver> : UmbracoIntegrationTest
{
    protected static void VerifyLogging(Mock<ILogger<TResolver>> loggerMock, LogLevel logLevel, string startOfMessage)
        => loggerMock.Verify(logger =>
            logger.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, _) => value.ToString()!.StartsWith(startOfMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

    protected class TestContentChangeStrategy : IContentChangeStrategy
    {
        public Task HandleAsync(IEnumerable<ContentIndexInfo> indexInfos, IEnumerable<ContentChange> changes, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public Task RebuildAsync(ContentIndexInfo indexInfo, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }

    protected abstract class IndexerBase : IIndexer
    {
        public Task AddOrUpdateAsync(string indexAlias, Guid id, UmbracoObjectTypes objectType, IEnumerable<Variation> variations, IEnumerable<IndexField> fields, ContentProtection? protection)
            => throw new NotImplementedException();

        public Task DeleteAsync(string indexAlias, IEnumerable<Guid> ids)
            => throw new NotImplementedException();

        public Task ResetAsync(string indexAlias)
            => throw new NotImplementedException();

        public Task<IndexMetadata> GetMetadataAsync(string indexAlias)
            => Task.FromResult(new IndexMetadata(0, HealthStatus.Healthy, "Test"));
    }

    protected abstract class SearcherBase : ISearcher
    {
        public Task<SearchResult> SearchAsync(
            string indexAlias,
            string? query = null,
            IEnumerable<Filter>? filters = null,
            IEnumerable<Facet>? facets = null,
            IEnumerable<Sorter>? sorters = null,
            string? culture = null,
            string? segment = null,
            AccessContext? accessContext = null,
            int skip = 0,
            int take = 10,
            int maxSuggestions = 0)
            => throw new NotImplementedException();
    }
}
