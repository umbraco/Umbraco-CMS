using System.Collections.Concurrent;
using System.Collections.Generic;
using Examine;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    internal class TestExamineManager : IExamineManager
    {
        private readonly ConcurrentDictionary<string, IIndexer> _indexers = new ConcurrentDictionary<string, IIndexer>();
        private readonly ConcurrentDictionary<string, ISearcher> _searchers = new ConcurrentDictionary<string, ISearcher>();

        public void AddIndexer(IIndexer indexer)
        {
            _indexers.TryAdd(indexer.Name, indexer);
        }

        public void AddSearcher(ISearcher searcher)
        {
            _searchers.TryAdd(searcher.Name, searcher);
        }
        
        public void Dispose()
        {
            //noop
        }

        public IIndexer GetIndexer(string indexerName)
        {
            return _indexers.TryGetValue(indexerName, out var indexer) ? indexer : null;
        }

        public ISearcher GetSearcher(string searcherName)
        {
            return _searchers.TryGetValue(searcherName, out var indexer) ? indexer : null;
        }

        public IReadOnlyDictionary<string, IIndexer> IndexProviders => _indexers;
    }
}
