using System.Collections.Concurrent;
using System.Collections.Generic;
using Examine;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    internal class TestExamineManager : IExamineManager
    {
        private readonly ConcurrentDictionary<string, IIndex> _indexers = new ConcurrentDictionary<string, IIndex>();
        private readonly ConcurrentDictionary<string, ISearcher> _searchers = new ConcurrentDictionary<string, ISearcher>();

        public void AddIndex(IIndex indexer)
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

        public bool TryGetIndex(string indexName, out IIndex index)
        {
            return _indexers.TryGetValue(indexName, out index);
        }

        public bool TryGetSearcher(string searcherName, out ISearcher searcher)
        {
            return _searchers.TryGetValue(searcherName, out searcher);
        }

        public IEnumerable<IIndex> Indexes => _indexers.Values;

        public IEnumerable<ISearcher> RegisteredSearchers => _searchers.Values;

        public IReadOnlyDictionary<string, IIndex> IndexProviders => _indexers;
    }
}
