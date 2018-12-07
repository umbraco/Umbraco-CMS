using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Core.Collections;

namespace Umbraco.Examine
{
    public abstract class IndexPopulator : IIndexPopulator
    {
        private readonly ConcurrentHashSet<string> _registeredIndexes = new ConcurrentHashSet<string>();

        public bool IsRegistered(string indexName)
        {
            return _registeredIndexes.Contains(indexName);
        }

        /// <summary>
        /// Registers an index for this populator
        /// </summary>
        /// <param name="indexName"></param>
        public void RegisterIndex(string indexName)
        {
            _registeredIndexes.Add(indexName);
        }

        public void Populate(params IIndex[] indexes)
        {
            PopulateIndexes(indexes.Where(x => IsRegistered(x.Name)));
        }

        protected abstract void PopulateIndexes(IEnumerable<IIndex> indexes);
    }
}
