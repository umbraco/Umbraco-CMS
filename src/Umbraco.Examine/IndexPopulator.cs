using System.Collections.Generic;
using System.Linq;
using Examine;

namespace Umbraco.Examine
{
    public abstract class IndexPopulator : IIndexPopulator
    {
        private readonly HashSet<string> _registeredIndexes = new HashSet<string>();

        /// <summary>
        /// Registers an index for this populator
        /// </summary>
        /// <param name="indexName"></param>
        public void RegisterIndex(string indexName)
        {
            _registeredIndexes.Add(indexName);
        }

        /// <summary>
        /// Returns a list of index names that his populate is associated with
        /// </summary>
        public IEnumerable<string> RegisteredIndexes => _registeredIndexes;

        public void Populate(params IIndex[] indexes)
        {
            PopulateIndexes(indexes.Where(x => RegisteredIndexes.Contains(x.Name)));
        }

        protected abstract void PopulateIndexes(IEnumerable<IIndex> indexes);
    }
}
