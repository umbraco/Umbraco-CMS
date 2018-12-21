using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Examine;

namespace Umbraco.Examine
{
    /// <summary>
    /// Utility to rebuild all indexes ensuring minimal data queries
    /// </summary>
    public class IndexRebuilder
    {
        private readonly IEnumerable<IIndexPopulator> _populators;
        public IExamineManager ExamineManager { get; }

        public IndexRebuilder(IExamineManager examineManager, IEnumerable<IIndexPopulator> populators)
        {
            _populators = populators;
            ExamineManager = examineManager;
        }

        public bool CanRebuild(IIndex index)
        {
            return _populators.Any(x => x.IsRegistered(index));
        }

        public void RebuildIndex(string indexName)
        {
            if (!ExamineManager.TryGetIndex(indexName, out var index))
                throw new InvalidOperationException($"No index found with name {indexName}");
            index.CreateIndex(); // clear the index
            foreach (var populator in _populators)
            {
                populator.Populate(index);
            }
        }

        public void RebuildIndexes(bool onlyEmptyIndexes)
        {
            var indexes = (onlyEmptyIndexes
                ? ExamineManager.Indexes.Where(x => !x.IndexExists())
                : ExamineManager.Indexes).ToArray();

            foreach (var index in indexes)
            {
                index.CreateIndex(); // clear the index
            }

            //run the populators in parallel against all indexes
            Parallel.ForEach(_populators, populator => populator.Populate(indexes));
        }

    }
}
