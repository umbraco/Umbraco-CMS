using System.Collections.Concurrent;
using System.Collections.Generic;
using Examine;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    internal class TestExamineManager : IExamineManager
    {
        private readonly ConcurrentDictionary<string, IIndexer> _indexers = new ConcurrentDictionary<string, IIndexer>();
        private readonly ConcurrentDictionary<string, ISearcher> _searchers = new ConcurrentDictionary<string, ISearcher>();

        public void AddIndexer(string name, IIndexer indexer)
        {
            _indexers.TryAdd(name, indexer);
        }

        public void AddSearcher(string name, ISearcher searcher)
        {
            _searchers.TryAdd(name, searcher);
        }

        public void DeleteFromIndexes(string nodeId)
        {
            //noop
        }

        public void DeleteFromIndexes(string nodeId, IEnumerable<IIndexer> providers)
        {
            //noop
        }

        public void Dispose()
        {
            //noop
        }

        public IIndexer GetIndexer(string indexerName)
        {
            return _indexers.TryGetValue(indexerName, out var indexer) ? indexer : null;
        }

        public ISearcher GetRegisteredSearcher(string searcherName)
        {
            return _searchers.TryGetValue(searcherName, out var indexer) ? indexer : null;
        }

        public void IndexAll(string indexCategory)
        {
            //noop
        }

        public void IndexItems(ValueSet[] nodes)
        {
            //noop
        }

        public void IndexItems(ValueSet[] nodes, IEnumerable<IIndexer> providers)
        {
            //noop
        }

        public void RebuildIndexes()
        {
            //noop
        }

        public IReadOnlyDictionary<string, IIndexer> IndexProviders => _indexers;
    }
}
